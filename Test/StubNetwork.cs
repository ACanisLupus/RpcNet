// Copyright by Artur Wolf

namespace Test;

using System.Net;
using RpcNet.Internal;

internal class StubNetwork(int maxReadLength, int maxReserveLength) : INetworkReader, INetworkWriter
{
    private readonly byte[] _buffer = new byte[65536];

    public int WriteIndex { get; private set; }
    private int ReadIndex { get; set; }

    public EndPoint BeginReading() => new IPEndPoint(0, 0);
    public ValueTask<EndPoint> BeginReadingAsync(CancellationToken cancellationToken) => ValueTask.FromResult(BeginReading());

    public void EndReading()
    {
    }

    public void BeginWriting()
    {
    }

    public ValueTask EndWritingAsync(EndPoint remoteEndPoint, CancellationToken cancellationToken) => ValueTask.CompletedTask;

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
