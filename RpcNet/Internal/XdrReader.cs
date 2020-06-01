namespace RpcNet.Internal
{
    using System;
    using System.Text;

    public class XdrReader : IXdrReader
    {
        private readonly Encoding encoding = Encoding.UTF8;
        private readonly INetworkReader networkReader;

        public XdrReader(INetworkReader networkReader) => this.networkReader = networkReader;

        public long ReadLong() => ((long)this.ReadInt() << 32) | (this.ReadInt() & 0xffffffff);
        public ulong ReadULong() => (ulong)this.ReadLong();
        public int ReadInt() => Utilities.ToInt32BigEndian(this.networkReader.Read(sizeof(int)));
        public uint ReadUInt() => (uint)this.ReadInt();
        public short ReadShort() => (short)this.ReadInt();
        public ushort ReadUShort() => (ushort)this.ReadInt();
        public sbyte ReadSByte() => (sbyte)this.ReadInt();
        public byte ReadByte() => (byte)this.ReadInt();
        public bool ReadBool() => this.ReadInt() != 0;
        public float ReadFloat() => Utilities.Int32BitsToSingle(this.ReadInt());
        public double ReadDouble() => BitConverter.Int64BitsToDouble(this.ReadLong());

        public byte[] ReadOpaque(int length)
        {
            int padding = Utilities.CalculateXdrPadding(length);
            byte[] value = new byte[length];
            int writeIndex = 0;

            while (length > 0)
            {
                ReadOnlySpan<byte> span = this.networkReader.Read(length);
                span.CopyTo(value.AsSpan(writeIndex, span.Length));
                writeIndex += span.Length;
                length -= span.Length;
            }

            while (padding > 0)
            {
                int paddingLength = this.networkReader.Read(padding).Length;
                padding -= paddingLength;
            }

            return value;
        }

        public byte[] ReadOpaque() => this.ReadOpaque(this.ReadInt());
        public string ReadString() => this.encoding.GetString(this.ReadOpaque());

        public bool[] ReadBoolArray(int length)
        {
            bool[] array = new bool[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = this.ReadBool();
            }

            return array;
        }

        public byte[] ReadByteArray(int length)
        {
            byte[] array = new byte[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = this.ReadByte();
            }

            return array;
        }

        public double[] ReadDoubleArray(int length)
        {
            double[] array = new double[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = this.ReadDouble();
            }

            return array;
        }

        public float[] ReadFloatArray(int length)
        {
            float[] array = new float[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = this.ReadFloat();
            }

            return array;
        }

        public int[] ReadIntArray(int length)
        {
            int[] array = new int[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = this.ReadInt();
            }

            return array;
        }

        public long[] ReadLongArray(int length)
        {
            long[] array = new long[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = this.ReadLong();
            }

            return array;
        }

        public sbyte[] ReadSByteArray(int length)
        {
            sbyte[] array = new sbyte[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = this.ReadSByte();
            }

            return array;
        }

        public short[] ReadShortArray(int length)
        {
            short[] array = new short[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = this.ReadShort();
            }

            return array;
        }

        public uint[] ReadUIntArray(int length)
        {
            uint[] array = new uint[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = this.ReadUInt();
            }

            return array;
        }

        public ulong[] ReadULongArray(int length)
        {
            ulong[] array = new ulong[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = this.ReadULong();
            }

            return array;
        }

        public ushort[] ReadUShortArray(int length)
        {
            ushort[] array = new ushort[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = this.ReadUShort();
            }

            return array;
        }

        public bool[] ReadBoolArray() => this.ReadBoolArray(this.ReadInt());
        public byte[] ReadByteArray() => this.ReadByteArray(this.ReadInt());
        public double[] ReadDoubleArray() => this.ReadDoubleArray(this.ReadInt());
        public float[] ReadFloatArray() => this.ReadFloatArray(this.ReadInt());
        public int[] ReadIntArray() => this.ReadIntArray(this.ReadInt());
        public long[] ReadLongArray() => this.ReadLongArray(this.ReadInt());
        public sbyte[] ReadSByteArray() => this.ReadSByteArray(this.ReadInt());
        public short[] ReadShortArray() => this.ReadShortArray(this.ReadInt());
        public uint[] ReadUIntArray() => this.ReadUIntArray(this.ReadInt());
        public ulong[] ReadULongArray() => this.ReadULongArray(this.ReadInt());
        public ushort[] ReadUShortArray() => this.ReadUShortArray(this.ReadInt());
    }
}
