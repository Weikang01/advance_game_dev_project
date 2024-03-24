using System;

public class DataViewer
{
    public static void write_ushort_le(byte[] buf, int offset, ushort value)
    {
        // value -> byte[]
        byte[] byte_value = BitConverter.GetBytes(value);
        // check if it's little endian or big endian
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(byte_value);
        }

        Array.Copy(byte_value, 0, buf, offset, byte_value.Length);
    }

    public static void write_uint_le(byte[] buf, int offset, uint value)
    {
        // value -> byte[]
        byte[] byte_value = BitConverter.GetBytes(value);
        // check if it's little endian or big endian
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(byte_value);
        }

        Array.Copy(byte_value, 0, buf, offset, byte_value.Length);
    }

    public static void write_bytes(byte[] dst, byte[] src, int offset)
    {
        Array.Copy(src, 0, dst, offset, src.Length);
    }

    public static ushort read_ushort_le(byte[] data, int offset)
    {
        int ret = (data[offset] | data[offset + 1] << 8);
        return (ushort)ret;
    }
}
