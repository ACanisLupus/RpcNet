namespace RpcNet
{
    using System;

    public interface IXdrWriter
    {
        void Write(bool value);
        void Write(byte value);
        void Write(double value);
        void Write(float value);
        void Write(int value);
        void Write(long value);
        void Write(sbyte value);
        void Write(short value);
        void Write(string value);
        void Write(uint value);
        void Write(ulong value);
        void Write(ushort value);
        void WriteFixedLengthArray(ReadOnlySpan<bool> array);
        void WriteFixedLengthArray(ReadOnlySpan<byte> array);
        void WriteFixedLengthArray(ReadOnlySpan<double> array);
        void WriteFixedLengthArray(ReadOnlySpan<float> array);
        void WriteFixedLengthArray(ReadOnlySpan<int> array);
        void WriteFixedLengthArray(ReadOnlySpan<long> array);
        void WriteFixedLengthArray(ReadOnlySpan<sbyte> array);
        void WriteFixedLengthArray(ReadOnlySpan<short> array);
        void WriteFixedLengthArray(ReadOnlySpan<uint> array);
        void WriteFixedLengthArray(ReadOnlySpan<ulong> array);
        void WriteFixedLengthArray(ReadOnlySpan<ushort> array);
        void WriteFixedLengthOpaque(ReadOnlySpan<byte> value);
        void WriteVariableLengthArray(ReadOnlySpan<bool> array);
        void WriteVariableLengthArray(ReadOnlySpan<byte> array);
        void WriteVariableLengthArray(ReadOnlySpan<double> array);
        void WriteVariableLengthArray(ReadOnlySpan<float> array);
        void WriteVariableLengthArray(ReadOnlySpan<int> array);
        void WriteVariableLengthArray(ReadOnlySpan<long> array);
        void WriteVariableLengthArray(ReadOnlySpan<sbyte> array);
        void WriteVariableLengthArray(ReadOnlySpan<short> array);
        void WriteVariableLengthArray(ReadOnlySpan<uint> array);
        void WriteVariableLengthArray(ReadOnlySpan<ulong> array);
        void WriteVariableLengthArray(ReadOnlySpan<ushort> array);
        void WriteVariableLengthOpaque(ReadOnlySpan<byte> value);
    }
}
