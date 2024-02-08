// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Text;

// Public for tests
public sealed class XdrReader : IXdrReader
{
    private readonly Encoding _encoding = Encoding.UTF8;
    private readonly INetworkReader _networkReader;

    public XdrReader(INetworkReader networkReader) => _networkReader = networkReader;

    public long ReadInt64() => ((long)ReadInt32() << 32) | (ReadInt32() & 0xffffffff);
    public ulong ReadUInt64() => (ulong)ReadInt64();
    public int ReadInt32() => Utilities.ToInt32BigEndian(_networkReader.Read(sizeof(int)));
    public uint ReadUInt32() => (uint)ReadInt32();
    public short ReadInt16() => (short)ReadInt32();
    public ushort ReadUInt16() => (ushort)ReadInt32();
    public sbyte ReadInt8() => (sbyte)ReadInt32();
    public byte ReadUInt8() => (byte)ReadInt32();
    public bool ReadBool() => ReadInt32() != 0;
    public float ReadFloat32() => BitConverter.Int32BitsToSingle(ReadInt32());
    public double ReadFloat64() => BitConverter.Int64BitsToDouble(ReadInt64());
    public string ReadString() => _encoding.GetString(ReadOpaque());

    public void ReadOpaque(byte[] array)
    {
        int length = array.Length;
        int padding = Utilities.CalculateXdrPadding(length);
        int writeIndex = 0;

        while (length > 0)
        {
            ReadOnlySpan<byte> span = _networkReader.Read(length);
            span.CopyTo(array.AsSpan(writeIndex, span.Length));
            writeIndex += span.Length;
            length -= span.Length;
        }

        _ = _networkReader.Read(padding);
    }

    public byte[] ReadOpaque()
    {
        byte[] array = new byte[ReadInt32()];
        ReadOpaque(array);
        return array;
    }
}
