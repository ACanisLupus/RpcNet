// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Text;

// Public for tests
public sealed class XdrWriter : IXdrWriter
{
    private readonly Encoding _encoding = Encoding.UTF8;
    private readonly INetworkWriter _networkWriter;

    public XdrWriter(INetworkWriter networkWriter) => _networkWriter = networkWriter;

    public void Write(long value)
    {
        Write((int)(value >> 32));
        Write((int)(value & 0xffffffff));
    }

    public void Write(ulong value) => Write((long)value);
    public void Write(int value) => Utilities.WriteBytesBigEndian(_networkWriter.Reserve(sizeof(int)), value);
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
            Span<byte> span = _networkWriter.Reserve(length);
            value.Slice(readIndex, span.Length).CopyTo(span);
            readIndex += span.Length;
            length -= span.Length;
        }

        FillWithZeros(_networkWriter.Reserve(padding));
    }

    public void WriteOpaque(ReadOnlySpan<byte> value)
    {
        Write(value.Length);
        WriteFixedLengthOpaque(value);
    }

    public void Write(string? value)
    {
        int length = value?.Length ?? 0;
        if ((value is null) || (length == 0))
        {
            Write(length);
            return;
        }

        WriteOpaque(_encoding.GetBytes(value));
    }

    private static void FillWithZeros(Span<byte> buffer)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = 0;
        }
    }
}
