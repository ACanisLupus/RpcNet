namespace RpcNet.Internal
{
    using System;
    using System.Text;

    public class XdrWriter : IXdrWriter
    {
        private readonly Encoding encoding = Encoding.UTF8;
        private readonly INetworkWriter networkWriter;

        public XdrWriter(INetworkWriter networkWriter) => this.networkWriter = networkWriter;

        public void Write(long value)
        {
            this.Write((int)(value >> 32));
            this.Write((int)(value & 0xffffffff));
        }

        public void Write(ulong value) => this.Write((long)value);
        public void Write(int value)
            => Utilities.WriteBytesBigEndian(this.networkWriter.Reserve(sizeof(int)), value);
        public void Write(uint value) => this.Write((int)value);
        public void Write(short value) => this.Write((int)value);
        public void Write(ushort value) => this.Write((int)value);
        public void Write(sbyte value) => this.Write((int)value);
        public void Write(byte value) => this.Write((int)value);
        public void Write(bool value) => this.Write(value ? 1 : 0);
        public void Write(float value) => this.Write(Utilities.SingleToInt32Bits(value));
        public void Write(double value) => this.Write(BitConverter.DoubleToInt64Bits(value));

        public void WriteFixedLengthOpaque(ReadOnlySpan<byte> value)
        {
            int length = value.Length;
            int padding = Utilities.CalculateXdrPadding(length);
            int readIndex = 0;

            while (length > 0)
            {
                Span<byte> span = this.networkWriter.Reserve(length);
                value.Slice(readIndex, span.Length).CopyTo(span);
                readIndex += span.Length;
                length -= span.Length;
            }

            this.FillWithZeros(this.networkWriter.Reserve(padding));
        }

        public void WriteVariableLengthOpaque(ReadOnlySpan<byte> value)
        {
            this.Write(value.Length);
            this.WriteFixedLengthOpaque(value);
        }

        public void Write(string value)
        {
            int length = value?.Length ?? 0;
            if (length == 0)
            {
                this.Write(length);
                return;
            }

            this.WriteVariableLengthOpaque(this.encoding.GetBytes(value));
        }

        public void WriteFixedLengthArray(ReadOnlySpan<long> array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                this.Write(array[i]);
            }
        }

        public void WriteFixedLengthArray(ReadOnlySpan<ulong> array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                this.Write(array[i]);
            }
        }

        public void WriteFixedLengthArray(ReadOnlySpan<int> array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                this.Write(array[i]);
            }
        }

        public void WriteFixedLengthArray(ReadOnlySpan<uint> array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                this.Write(array[i]);
            }
        }

        public void WriteFixedLengthArray(ReadOnlySpan<short> array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                this.Write(array[i]);
            }
        }

        public void WriteFixedLengthArray(ReadOnlySpan<ushort> array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                this.Write(array[i]);
            }
        }

        public void WriteFixedLengthArray(ReadOnlySpan<sbyte> array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                this.Write(array[i]);
            }
        }

        public void WriteFixedLengthArray(ReadOnlySpan<byte> array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                this.Write(array[i]);
            }
        }

        public void WriteFixedLengthArray(ReadOnlySpan<bool> array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                this.Write(array[i]);
            }
        }

        public void WriteFixedLengthArray(ReadOnlySpan<float> array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                this.Write(array[i]);
            }
        }

        public void WriteFixedLengthArray(ReadOnlySpan<double> array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                this.Write(array[i]);
            }
        }

        public void WriteVariableLengthArray(ReadOnlySpan<long> array)
        {
            this.Write(array.Length);
            this.WriteFixedLengthArray(array);
        }

        public void WriteVariableLengthArray(ReadOnlySpan<ulong> array)
        {
            this.Write(array.Length);
            this.WriteFixedLengthArray(array);
        }

        public void WriteVariableLengthArray(ReadOnlySpan<int> array)
        {
            this.Write(array.Length);
            this.WriteFixedLengthArray(array);
        }

        public void WriteVariableLengthArray(ReadOnlySpan<uint> array)
        {
            this.Write(array.Length);
            this.WriteFixedLengthArray(array);
        }

        public void WriteVariableLengthArray(ReadOnlySpan<short> array)
        {
            this.Write(array.Length);
            this.WriteFixedLengthArray(array);
        }

        public void WriteVariableLengthArray(ReadOnlySpan<ushort> array)
        {
            this.Write(array.Length);
            this.WriteFixedLengthArray(array);
        }

        public void WriteVariableLengthArray(ReadOnlySpan<sbyte> array)
        {
            this.Write(array.Length);
            this.WriteFixedLengthArray(array);
        }

        public void WriteVariableLengthArray(ReadOnlySpan<byte> array)
        {
            this.Write(array.Length);
            this.WriteFixedLengthArray(array);
        }

        public void WriteVariableLengthArray(ReadOnlySpan<bool> array)
        {
            this.Write(array.Length);
            this.WriteFixedLengthArray(array);
        }

        public void WriteVariableLengthArray(ReadOnlySpan<float> array)
        {
            this.Write(array.Length);
            this.WriteFixedLengthArray(array);
        }

        public void WriteVariableLengthArray(ReadOnlySpan<double> array)
        {
            this.Write(array.Length);
            this.WriteFixedLengthArray(array);
        }

        private void FillWithZeros(Span<byte> buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = 0;
            }
        }
    }
}
