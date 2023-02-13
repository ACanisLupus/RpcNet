// Copyright by Artur Wolf

namespace RpcNet;

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
    void WriteOpaque(ReadOnlySpan<byte> value);
}
