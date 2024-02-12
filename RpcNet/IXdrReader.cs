// Copyright by Artur Wolf

namespace RpcNet;

public interface IXdrReader
{
    bool ReadBool();
    float ReadFloat32();
    double ReadFloat64();
    sbyte ReadInt8();
    short ReadInt16();
    int ReadInt32();
    long ReadInt64();
    string ReadString();
    byte ReadUInt8();
    ushort ReadUInt16();
    ulong ReadUInt64();
    uint ReadUInt32();
    byte[] ReadOpaque();
    void ReadFixedLengthOpaque(byte[] array);
}
