namespace RpcNet
{
    public interface IXdrReader
    {
        bool ReadBool();
        byte ReadByte();
        double ReadDouble();
        float ReadFloat();
        int ReadInt();
        long ReadLong();
        sbyte ReadSByte();
        short ReadShort();
        string ReadString();
        uint ReadUInt();
        ulong ReadULong();
        ushort ReadUShort();
        void ReadOpaque(byte[] array);
        void ReadBoolArray(bool[] array);
        void ReadByteArray(byte[] array);
        void ReadDoubleArray(double[] array);
        void ReadFloatArray(float[] array);
        void ReadIntArray(int[] array);
        void ReadLongArray(long[] array);
        void ReadSByteArray(sbyte[] array);
        void ReadShortArray(short[] array);
        void ReadUIntArray(uint[] array);
        void ReadULongArray(ulong[] array);
        void ReadUShortArray(ushort[] array);
        byte[] ReadOpaque();
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
