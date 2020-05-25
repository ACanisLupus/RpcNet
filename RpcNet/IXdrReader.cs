namespace RpcNet
{
    public interface IXdrReader
    {
        bool ReadBool();
        byte ReadByte();
        double ReadDouble();
        byte[] ReadOpaque(int length);
        float ReadFloat();
        int ReadInt();
        long ReadLong();
        sbyte ReadSByte();
        short ReadShort();
        string ReadString();
        uint ReadUInt();
        ulong ReadULong();
        ushort ReadUShort();
        byte[] ReadOpaque();
        bool[] ReadBoolArray(int length);
        byte[] ReadByteArray(int length);
        double[] ReadDoubleArray(int length);
        float[] ReadFloatArray(int length);
        int[] ReadIntArray(int length);
        long[] ReadLongArray(int length);
        sbyte[] ReadSByteArray(int length);
        short[] ReadShortArray(int length);
        uint[] ReadUIntArray(int length);
        ulong[] ReadULongArray(int length);
        ushort[] ReadUShortArray(int length);
        bool[] ReadBoolArray();
        byte[] ReadByteArray();
        double[] ReadDoubleArray();
        float[] ReadFloatArray();
        int[] ReadIntArray();
        long[] ReadLongArray();
        sbyte[] ReadSByteArray();
        short[] ReadShortArray();
        uint[] ReadUIntArray();
        ulong[] ReadULongArray();
        ushort[] ReadUShortArray();
    }
}
