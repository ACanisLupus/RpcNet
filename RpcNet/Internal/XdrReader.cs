namespace RpcNet.Internal
{
    using System;
    using System.Text;

    public class XdrReader : IXdrReader
    {
        private readonly Encoding encoding = Encoding.UTF8;
        private readonly INetworkReader networkReader;

        public XdrReader(INetworkReader networkReader)
        {
            this.networkReader = networkReader;
        }

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
        public string ReadString() => this.encoding.GetString(this.ReadOpaque());

        public void ReadOpaque(byte[] array)
        {
            int length = array.Length;
            int padding = Utilities.CalculateXdrPadding(length);
            int writeIndex = 0;

            while (length > 0)
            {
                ReadOnlySpan<byte> span = this.networkReader.Read(length);
                span.CopyTo(array.AsSpan(writeIndex, span.Length));
                writeIndex += span.Length;
                length -= span.Length;
            }

            _ = this.networkReader.Read(padding);
        }

        public void ReadBoolArray(bool[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = this.ReadBool();
            }
        }

        public void ReadByteArray(byte[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = this.ReadByte();
            }
        }

        public void ReadDoubleArray(double[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = this.ReadDouble();
            }
        }

        public void ReadFloatArray(float[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = this.ReadFloat();
            }
        }

        public void ReadIntArray(int[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = this.ReadInt();
            }
        }

        public void ReadLongArray(long[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = this.ReadLong();
            }
        }

        public void ReadSByteArray(sbyte[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = this.ReadSByte();
            }
        }

        public void ReadShortArray(short[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = this.ReadShort();
            }
        }

        public void ReadUIntArray(uint[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = this.ReadUInt();
            }
        }

        public void ReadULongArray(ulong[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = this.ReadULong();
            }
        }

        public void ReadUShortArray(ushort[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = this.ReadUShort();
            }
        }

        public byte[] ReadOpaque()
        {
            var array = new byte[this.ReadInt()];
            this.ReadOpaque(array);
            return array;
        }

        public bool[] ReadBoolArray()
        {
            var array = new bool[this.ReadInt()];
            this.ReadBoolArray(array);
            return array;
        }

        public byte[] ReadByteArray()
        {
            var array = new byte[this.ReadInt()];
            this.ReadByteArray(array);
            return array;
        }

        public double[] ReadDoubleArray()
        {
            var array = new double[this.ReadInt()];
            this.ReadDoubleArray(array);
            return array;
        }

        public float[] ReadFloatArray()
        {
            var array = new float[this.ReadInt()];
            this.ReadFloatArray(array);
            return array;
        }

        public int[] ReadIntArray()
        {
            var array = new int[this.ReadInt()];
            this.ReadIntArray(array);
            return array;
        }

        public long[] ReadLongArray()
        {
            var array = new long[this.ReadInt()];
            this.ReadLongArray(array);
            return array;
        }

        public sbyte[] ReadSByteArray()
        {
            var array = new sbyte[this.ReadInt()];
            this.ReadSByteArray(array);
            return array;
        }

        public short[] ReadShortArray()
        {
            var array = new short[this.ReadInt()];
            this.ReadShortArray(array);
            return array;
        }

        public uint[] ReadUIntArray()
        {
            var array = new uint[this.ReadInt()];
            this.ReadUIntArray(array);
            return array;
        }

        public ulong[] ReadULongArray()
        {
            var array = new ulong[this.ReadInt()];
            this.ReadULongArray(array);
            return array;
        }

        public ushort[] ReadUShortArray()
        {
            var array = new ushort[this.ReadInt()];
            this.ReadUShortArray(array);
            return array;
        }
    }
}
