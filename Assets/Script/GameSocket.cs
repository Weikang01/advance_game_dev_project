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
    public List<GameMessage.IngameMessage> messages;
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
            messages = new List<GameMessage.IngameMessage>();
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
                byte[] bytes = new byte[4096];

                int i = socket.Receive(bytes);
                if (i <= 0)
                {
                    socket.Close();
                    return;
                }

                if (bytes.Length > 4)
                {
                    SplitMessage(bytes, 0);
                }
            } catch (Exception e)
            {
                Debug.Log(e);
            }
        }
    }

    private void SplitMessage(byte[] bytes, int index)
    {
        while (true)
        {
            byte[] head = new byte[6];
            int headLengthIndex = index + 6;
            Array.Copy(bytes, index, head, 0, 4);

            short otherClientID = BitConverter.ToInt16(head, 0);

            if (otherClientID > 0)
            {
                short length = BitConverter.ToInt16(head, 2);
                short messageType = BitConverter.ToInt16(head, 4);

                byte[] data = new byte[length];
                Array.Copy(bytes, headLengthIndex, data, 0, length);
                GameMessage.IngameMessage wm = new GameMessage.IngameMessage();

                wm = (GameMessage.IngameMessage)BytesToStruct(data, wm.GetType());

                Debug.Log("Receive message: " + wm.playerPosX + " " + wm.playerPosY);

                messages.Add(wm);

                index = headLengthIndex + length;
            } else
            {
                break;
            }
        }
    }

    public void SendMessage(short clientID, object obj)
    {
        if (!socket.Connected)
        {
            socket.Close();
            return;
        }
        try
        {
            short size = (short)Marshal.SizeOf(obj);
            byte[] head = BitConverter.GetBytes(size);
            byte[] data = StructToBytes(obj);

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
