using UnityEngine;
using System.Threading;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

public class GameSocket
{
    private Socket socket;
    public Dictionary<short, List<GameMessage.clientMessage>> client_messages;
    public List<Dictionary<string, string>> system_messages;
    private static GameSocket instance;
    public static GameSocket GetInstance()
    {
        if (instance == null)
        {
            instance = new GameSocket();
        }
        return instance;
    }

    private GameSocket()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        // server ip address
        IPAddress iPAddress = IPAddress.Parse("127.0.0.1");
        // server ip endpoint
        IPEndPoint clientEndPoint = new IPEndPoint(iPAddress, 5009);


        IAsyncResult result = socket.BeginConnect(clientEndPoint, new AsyncCallback(connectCallback), socket);

        bool success = result.AsyncWaitHandle.WaitOne(5000, true);
        if (success)
        {
            client_messages = new Dictionary<short, List<GameMessage.clientMessage>>();
            system_messages = new List<Dictionary<string, string>>();
            Thread thread = new Thread(new ThreadStart(ReceiveSocket));
            thread.IsBackground = true;
            thread.Start();
        }
    }

    private void connectCallback(IAsyncResult result)
    {
        Debug.Log("Connected to server");
    }

    private void ReceiveSocket()
    {
        while (true)
        {
            if (!socket.Connected)
            {
                Debug.Log("Failed to connect server!");
                socket.Close();
                return;
            }

            try
            {
                byte[] headerBytes = new byte[GameMessage.MessageHeader.GetSize()];
                int bytesRead = socket.Receive(headerBytes, headerBytes.Length, SocketFlags.None);

                if (bytesRead == 0)
                {
                    Debug.Log("Connection closed by server.");
                    socket.Close();
                    return;
                }

                GameMessage.MessageHeader header = GameMessage.MessageHeader.FromBytes(headerBytes);

                Debug.Log("Receive message header: " + header.clientID + " " + header.messageLength + " ");

                if (header.messageLength > 0)
                {
                    byte[] messageBytes = new byte[header.messageLength];
                    bytesRead = socket.Receive(messageBytes, header.messageLength, SocketFlags.None);

                    if (bytesRead == 0)
                    {
                        Debug.Log("Connection closed by server.");
                        socket.Close();
                        return;
                    }

                    if (header.messageType == (short)GameMessage.MessageType.CLIENT_MESSAGE)
                    {
                        if (!client_messages.ContainsKey(header.clientID))
                        {
                            client_messages.Add(header.clientID, new List<GameMessage.clientMessage>());
                        }

                        client_messages[header.clientID].Add(GameMessage.clientMessage.FromBytes(messageBytes));
                    }
                    else if (header.messageType == (short)GameMessage.MessageType.SYSTEM_MESSAGE)
                    {
                        // converse message bytes to JSON
                        string message = System.Text.Encoding.UTF8.GetString(messageBytes);
                        system_messages.Add(JsonConvert.DeserializeObject<Dictionary<string, string>>(message));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
    }

    public void SendMessage(short clientID, object body)
    {
        if (!socket.Connected)
        {
            socket.Close();
            return;
        }
        try
        {
            short size = (short)Marshal.SizeOf(body);
            GameMessage.MessageHeader header = new GameMessage.MessageHeader(clientID, size);
            byte[] head = StructToBytes(header);
            byte[] data = StructToBytes(body);

            //Debug.Log("Header size: " + head.Length + "\tBody size: " + data.Length);

            byte[] newByte = new byte[head.Length + data.Length];
            Array.Copy(head, 0, newByte, 0, head.Length);
            Array.Copy(data, 0, newByte, head.Length, data.Length);

            IAsyncResult asyncResult = socket.BeginSend(newByte, 0, newByte.Length, SocketFlags.None, new AsyncCallback(sendCallback), socket);

            bool success = asyncResult.AsyncWaitHandle.WaitOne(5000, true);

            if (!success)
            {
                socket.Close();
                Debug.Log("Time out!");
            }

        } catch (Exception e)
        {
            Debug.Log("send message error: " + e);
        }
    }

    public void SendScreenShot(short clientID, short width, short height, byte[] data)
    {
        if (!socket.Connected)
        {
            socket.Close();
            return;
        }
        try
        {
            GameMessage.MessageHeader header = new GameMessage.MessageHeader(clientID, (short)data.Length, (short)GameMessage.MessageType.SCREENSHOT);
            GameMessage.ScreenShotHeaderMessage screenShotHeaderMessage = new GameMessage.ScreenShotHeaderMessage(width, height);
            byte[] head = StructToBytes(header);
            byte[] screenshotheader = StructToBytes(screenShotHeaderMessage);
            byte[] newByte = new byte[head.Length + screenshotheader.Length + data.Length];
            Array.Copy(head, 0, newByte, 0, head.Length);
            Array.Copy(screenshotheader, 0, newByte, head.Length, screenshotheader.Length);
            Array.Copy(data, 0, newByte, head.Length + screenshotheader.Length, data.Length);

            IAsyncResult asyncResult = socket.BeginSend(newByte, 0, newByte.Length, SocketFlags.None, new AsyncCallback(sendCallback), socket);

            bool success = asyncResult.AsyncWaitHandle.WaitOne(5000, true);

            if (!success)
            {
                socket.Close();
                Debug.Log("Time out!");
            }
        }
         catch (Exception e)
        {
            Debug.Log("send screenshot message error: " + e);
        }
    }

    private void sendCallback(IAsyncResult ar)
    {
        
    }

    public byte[] StructToBytes(object obj)
    {
        int size = Marshal.SizeOf(obj);
        IntPtr ptr = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(obj, ptr, false);
            byte[] data = new byte[size];
            Marshal.Copy(ptr, data, 0, size);
            return data;
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }

    public object BytesToStruct(byte[] data, Type structType)
    {
        int size = Marshal.SizeOf(structType);
        IntPtr buffer = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.Copy(data, 0, buffer, size);
            return Marshal.PtrToStructure(buffer, structType);
        }
        finally { Marshal.FreeHGlobal(buffer); }
    }

    public void Close(short clientID)
    {
        if (socket != null && socket.Connected)
        {
            try
            {
                // Create a disconnect message and send it to the server
                GameMessage.clientMessage disconnectMessage = new GameMessage.clientMessage((short)GameMessage.ActionType.QUIT, clientID);
                SendMessage(clientID, disconnectMessage);

                // Close the socket
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception e)
            {
                Debug.Log("Error during disconnect: " + e);
            }
        }
    }
}
