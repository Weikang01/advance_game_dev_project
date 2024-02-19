using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TCPPackager
{
    private const int HEADER_SIZE = 2;
    public static byte[] pack(byte[] cmd_data)
    {
        int len = cmd_data.Length + HEADER_SIZE;
        if (len > ushort.MaxValue)
        {
            return null;
        }

        byte[] cmd = new byte[len];
        DataViewer.write_ushort_le(cmd, 0, (ushort)cmd_data.Length);
        DataViewer.write_bytes(cmd, cmd_data, HEADER_SIZE);
        return cmd;
    }

    public static bool read_header(byte[] data, int data_len, out int header_len, out int package_len)
    {
        if (data_len < HEADER_SIZE)
        {
            header_len = 0;
            package_len = 0;
            return false;
        }

        package_len = (data[0] | (data[1] << 8));
        header_len = HEADER_SIZE;

        return true;
    }
}
