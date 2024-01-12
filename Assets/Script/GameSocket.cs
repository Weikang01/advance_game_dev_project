using UnityEngine;
using System.Collections;
using System.Threading;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System;
using System.Runtime.InteropServices;
using UnityEditor.PackageManager;

public class GameSocket
{
    private Socket socket;
    public Dictionary<short, List<GameMessage.IngameMessage>> messages;
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
        IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, 5009);
        IAsyncResult result = socket.BeginConnect(iPEndPoint, new AsyncCallback(connectCallback), socket);

        bool success = result.AsyncWaitHandle.WaitOne(5000, true);
        if (success)
        {
            messages = new Dictionary<short, List<GameMessage.IngameMessage>>();
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
                    bytesRead = socket.Receive(messageBytes, messageBytes.Length, SocketFlags.None);

                    if (bytesRead == 0)
                    {
                        Debug.Log("Connection closed by server.");
                        socket.Close();
                        return;
                    }
                    if (!messages.ContainsKey(header.clientID))
                    {
                        messages.Add(header.clientID, new List<GameMessage.IngameMessage>());
                    }

                    messages[header.clientID].Add(GameMessage.IngameMessage.FromBytes(messageBytes));
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
}
