using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
public enum ProtoType
{
    PROTO_JSON = 0,
    PROTO_BUF = 1,
}

public class cmd_msg
{
    public int stype;
    public int ctype;
    public byte[] body;  // protobuf or json (utf-8)
}


public class ProtoManager
{
    public static ProtoType proto_type = ProtoType.PROTO_JSON;

    private const int HEADER_SIZE = 8;  // 2 bytes for stype, 2 bytes for ctype, 4 bytes for utag


    private static byte[] SerializeProtobuf(Google.Protobuf.IMessage message)
    {
        if (message == null)
            return null;

        byte[] result = new byte[message.CalculateSize()];
        CodedOutputStream output = new CodedOutputStream(result);
        message.WriteTo(output);
        return result;
    }

    public static T DeserializeProtobuf<T>(byte[] data) where T : Google.Protobuf.IMessage, new()
    {
        CodedInputStream input = new CodedInputStream(data);
        T result = new T();

        result.MergeFrom(input);
        return result;
    }

    public static byte[] PackProtobufCmd(int stype, int ctype, Google.Protobuf.IMessage message)
    {
        int cmd_len = HEADER_SIZE;
        byte[] cmd_body = null;

        if (message != null)
        {
            cmd_body = SerializeProtobuf(message);
            cmd_len += cmd_body.Length;
        }

        byte[] cmd = new byte[cmd_len];
        // stype, ctype, utag(reserve)
        DataViewer.write_ushort_le(cmd, 0, (ushort)stype);
        DataViewer.write_ushort_le(cmd, 2, (ushort)ctype);
        if (message != null)
            DataViewer.write_bytes(cmd, cmd_body, HEADER_SIZE);

        return cmd;
    }

    public static byte[] PackJsonCmd(int stype, int ctype, string body)
    {
        int cmd_len = HEADER_SIZE;
        byte[] cmd_body = null;
        if (body.Length > 0) // utf-8
        {
            cmd_body = Encoding.UTF8.GetBytes(body);
            cmd_len += cmd_body.Length;
        }

        byte[] cmd = new byte[cmd_len];
        // stype, ctype, utag(reserve)
        DataViewer.write_ushort_le(cmd, 0, (ushort)stype);
        DataViewer.write_ushort_le(cmd, 2, (ushort)ctype);
        DataViewer.write_bytes(cmd, cmd_body, HEADER_SIZE);

        return cmd;
    }

    public static bool DecodeCmdMsg(byte[] in_data, int offset, int in_len, out cmd_msg out_msg)
    {
        out_msg = null;

        if (in_len < HEADER_SIZE)
            return false;

        out_msg = new cmd_msg();

        out_msg.stype = DataViewer.read_ushort_le(in_data, offset);
        out_msg.ctype = DataViewer.read_ushort_le(in_data, offset + 2);
        out_msg.body = null;

        if (in_len == HEADER_SIZE)
            return true;

        out_msg.body = new byte[in_len - HEADER_SIZE];
        Array.Copy(in_data, offset + HEADER_SIZE, out_msg.body, 0, in_len - HEADER_SIZE);

        return true;
    }
}
