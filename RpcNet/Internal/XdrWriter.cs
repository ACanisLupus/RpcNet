// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Text;

internal sealed class XdrWriter(INetworkWriter networkWriter) : IXdrWriter
{
    private readonly Encoding _encoding = Encoding.UTF8;

    public void Write(long value)
    {
        Write((int)(value >> 32));
        Write((int)(value & 0xffffffff));
    }

    public void Write(ulong value) => Write((long)value);
    public void Write(int value) => Utilities.WriteBytesBigEndian(networkWriter.Reserve(sizeof(int)), value);
    public void Write(uint value) => Write((int)value);
    public void Write(short value) => Write((int)value);
    public void Write(ushort value) => Write((int)value);
    public void Write(sbyte value) => Write((int)value);
    public void Write(byte value) => Write((int)value);
    public void Write(bool value) => Write(value ? 1 : 0);
    public void Write(float value) => Write(BitConverter.SingleToInt32Bits(value));
    public void Write(double value) => Write(BitConverter.DoubleToInt64Bits(value));

    public void WriteFixedLengthOpaque(ReadOnlySpan<byte> value)
    {
        int length = value.Length;
        int padding = Utilities.CalculateXdrPadding(length);
        int readIndex = 0;

        while (length > 0)
        {
            Span<byte> span = networkWriter.Reserve(length);
            value.Slice(readIndex, span.Length).CopyTo(span);
            readIndex += span.Length;
            length -= span.Length;
        }

        FillWithZeros(networkWriter.Reserve(padding));
    }

    public void WriteOpaque(ReadOnlySpan<byte> value)
    {
        Write(value.Length);
        WriteFixedLengthOpaque(value);
    }

    public void Write(string? value)
    {
        int length = value?.Length ?? 0;
        if (value is null || (length == 0))
        {
            Write(length);
            return;
        }

        WriteOpaque(_encoding.GetBytes(value));
    }

    private static void FillWithZeros(Span<byte> buffer) => buffer.Clear();
}
