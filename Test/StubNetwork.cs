// Copyright by Artur Wolf

namespace Test;

using System.Net;
using RpcNet.Internal;

internal class StubNetwork(int maxReadLength, int maxReserveLength) : INetworkReader, INetworkWriter
{
    private readonly byte[] _buffer = new byte[65536];

    private int ReadIndex { get; set; }
    public int WriteIndex { get; private set; }

    public EndPoint BeginReading() => new IPEndPoint(0, 0);

    public void EndReading()
    {
    }

    public void BeginWriting()
    {
    }

    public void EndWriting(EndPoint remoteEndPoint)
    {
    }

    public void Reset()
    {
        ReadIndex = 0;
        WriteIndex = 0;
    }

    public ReadOnlySpan<byte> Read(int length)
    {
        length = Math.Min(length, maxReadLength);
        Span<byte> span = _buffer.AsSpan(ReadIndex, length);
        ReadIndex += length;
        return span;
    }

    public Span<byte> Reserve(int length)
    {
        length = Math.Min(length, maxReserveLength);
        Span<byte> span = _buffer.AsSpan(WriteIndex, length);
        WriteIndex += length;
        return span;
    }
}
