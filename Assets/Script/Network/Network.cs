using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;



public class Network : UnitySingleton<Network>
{
    private string serverIP = "127.0.0.1";
    private int port = 6080;

    private Socket client_socket = null;
    private bool is_connected = false;
    private Thread receive_thread = null;

    private const int SESSION_RECV_LEN = 8192;
    private byte[] recv_buffer = new byte[SESSION_RECV_LEN];
    private int recved = 0;
    private byte[] long_pkg = null;
    private int long_pkg_len = 0;

    // event queue
    private Queue<cmd_msg> event_queue = new Queue<cmd_msg>();
    // event listener, stype --> listener
    public delegate void cmd_msg_handler(cmd_msg msg);
    // map stype --> listener
    private Dictionary<int, cmd_msg_handler> event_listeners = new Dictionary<int, cmd_msg_handler>();

    private static bool is_closed = false;

    // Start is called before the first frame update
    void Start()
    {
        connectedToServer();

        // Test
        //this.Invoke("Test", 2.0f);
    }

    void Update()
    {
        lock (this.event_queue)
        {
            while (this.event_queue.Count > 0)
            {
                cmd_msg msg = this.event_queue.Dequeue();
                if (this.event_listeners.ContainsKey(msg.stype))
                {
                    this.event_listeners[msg.stype](msg);
                }
            }
        }
    }

    void OnDestroy()
    {
        this.closeConnection();
        is_closed = true;
    }

    private void OnApplicationQuit()
    {
        this.closeConnection();
    }

    private void on_receive_tcp_cmd(byte[] payload, int offset, int len)
    {
        cmd_msg msg = new cmd_msg();
        ProtoManager.DecodeCmdMsg(payload, offset, len, out msg);
        if (msg != null)
        {
            // test
            /*LoginRes res = protoManager.DeserializeProtobuf<LoginRes>(msg.body);
            Debug.Log("Receive: " + res.Status);*/
            lock(this.event_queue)
            {
                this.event_queue.Enqueue(msg);
            }
        }
    }

    private void on_receive_tcp_data()
    {
        byte[] package_data = this.long_pkg != null ? this.long_pkg : this.recv_buffer;

        while (this.recved > 0)
        {
            int payload_size = 0;
            int head_size = 0;

            if (!TCPPackager.read_header(package_data, this.recved, out head_size, out payload_size))
                break;

            if (this.recved < payload_size + head_size)
                break;

            on_receive_tcp_cmd(package_data, head_size, payload_size);

            if (this.recved > payload_size + head_size)
            {
                this.recv_buffer = new byte[SESSION_RECV_LEN];
                Array.Copy(package_data, payload_size, this.recv_buffer, 0, this.recved - payload_size);
            }

            this.recved -= payload_size + head_size;

            if (this.recved == 0 && this.long_pkg != null)
            {
                this.long_pkg = null;
                this.long_pkg_len = 0;
            }
        }
    }

    private void receiveWorker()
    {
        if (this.is_connected == false)
            return;

        while (true)
        {
            if (!this.client_socket.Connected)
                break;

            try
            {
                int receive_len = 0;
                if (this.recved < SESSION_RECV_LEN)
                {
                    receive_len = this.client_socket.Receive(this.recv_buffer, this.recved, SESSION_RECV_LEN - this.recved, SocketFlags.None);
                }
                else
                {
                    if (this.long_pkg == null)
                    {
                        int package_size;
                        int header_size;
                        TCPPackager.read_header(this.recv_buffer, this.recved, out header_size, out package_size);
                        this.long_pkg_len = package_size;
                        this.long_pkg = new byte[header_size];
                        Array.Copy(this.recv_buffer, 0, this.long_pkg, 0, this.recved);
                    }
                    receive_len = this.client_socket.Receive(this.long_pkg, this.recved, this.long_pkg_len - this.recved, SocketFlags.None);
                }

                if (receive_len > 0)
                {
                    this.recved += receive_len;
                    this.on_receive_tcp_data();
                }
            }
            catch (System.Exception e)
            {
                if (this.client_socket != null && this.client_socket.Connected)
                {
                    try
                    {
                        this.client_socket.Shutdown(SocketShutdown.Both);
                    }
                    catch (SocketException se)
                    {
                        // Handle the case where the socket is already shut down or closed
                        Debug.Log($"SocketException while shutting down: {se}");
                    }
                    catch (ObjectDisposedException ode)
                    {
                        // Handle the case where the socket has been disposed
                        Debug.Log($"ObjectDisposedException while shutting down: {ode}");
                    }

                    this.client_socket.Close();
                }
                this.is_connected = false;
            }
        }
    }

    private void onConnectSuccess(IAsyncResult result)
    {
        try
        {
            Socket socket = (Socket)result.AsyncState;
            socket.EndConnect(result);
            this.is_connected = true;
            this.receive_thread = new Thread(new ThreadStart(this.receiveWorker));
            this.receive_thread.Start();

            Debug.Log("connect success: " + this.serverIP + ":" + this.port);
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
            this.onConnectError(e.Message);
            this.is_connected = false;
        }
    }

    private void onConnectError(string error)
    {
    }

    private void connectedToServer()
    {
        try
        {
            this.client_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress iPAddress = IPAddress.Parse(this.serverIP);
            IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, this.port);
            IAsyncResult result = this.client_socket.BeginConnect(iPEndPoint, new AsyncCallback(this.onConnectSuccess), this.client_socket);
            bool is_timeout = result.AsyncWaitHandle.WaitOne(5000, true);
            if (!is_timeout)
            {
                this.client_socket.Close();
                Debug.Log("Failed to connect to server");
            }
        } catch (System.Exception e)
        {
            Debug.LogException(e);
            this.onConnectError(e.Message);
        }
    }

    void closeConnection()
    {
        if (!this.is_connected)
            return;

        if (this.receive_thread != null)
            this.receive_thread.Abort();

        if (this.client_socket != null && this.client_socket.Connected)
        {
            this.client_socket.Close();
        }
        this.is_connected = false;
    }

    private void onSendData(IAsyncResult result)
    {
        try
        {
            Socket socket = (Socket)result.AsyncState;
            socket.EndSend(result);
        } catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }

    public void sendProtoBufCmd(int stype, int ctype, Google.Protobuf.IMessage body)
    {
        byte[] cmd_data = ProtoManager.PackProtobufCmd(stype, ctype, body);
        if (cmd_data == null)
        {
            Debug.Log("sendProtoBufCmd pack_protobuf_cmd failed");
            return;
        }

        byte[] cmd = TCPPackager.pack(cmd_data);
        if (cmd == null)
        {
            Debug.Log("sendProtoBufCmd TCPPackager.pack failed");
            return;
        }

        if (this.client_socket != null && this.client_socket.Connected)
        {
            this.client_socket.BeginSend(cmd, 0, cmd.Length, SocketFlags.None, new AsyncCallback(this.onSendData), this.client_socket);
        }
    }

    public void sendJsonCmd(int stype, int ctype, string body)
    {
        byte[] cmd_data = ProtoManager.PackJsonCmd(stype, ctype, body);
        if (cmd_data == null)
        {
            Debug.Log("sendJsonCmd pack_json_cmd failed");
            return;
        }
        byte[] cmd = TCPPackager.pack(cmd_data);
        if (cmd == null)
        {
            Debug.Log("sendJsonCmd TCPPackager.pack failed");
            return;
        }
        if (this.client_socket != null && this.client_socket.Connected)
        {
            this.client_socket.BeginSend(cmd, 0, cmd.Length, SocketFlags.None, this.onSendData, this.client_socket);
        }
    }

    public void AddServiceListener(int stype, cmd_msg_handler handler)
    {
        if (this.event_listeners.ContainsKey(stype))
        {
            this.event_listeners[stype] += handler;
        }
        else
        {
            this.event_listeners.Add(stype, handler);
        }
    }

    public void RemoveServiceListener(int stype, cmd_msg_handler handler)
    {
        if (this.event_listeners.ContainsKey(stype))
        {
            this.event_listeners[stype] -= handler;
            if (this.event_listeners[stype] == null)
            {
                this.event_listeners.Remove(stype);
            }
        }
    }
}
