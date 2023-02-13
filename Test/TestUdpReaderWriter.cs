// Copyright by Artur Wolf

namespace Test;

using System.Net;
using System.Net.Sockets;
using NUnit.Framework;
using RpcNet;
using RpcNet.Internal;

[TestFixture]
internal class TestUdpReaderWriter
{
    private UdpClient _client;
    private UdpReader _reader;
    private IPEndPoint _remoteIpEndPoint;
    private UdpClient _server;
    private UdpWriter _writer;

    [SetUp]
    public void SetUp()
    {
        _server = new UdpClient(0);

        int port = ((IPEndPoint)_server.Client.LocalEndPoint).Port;
        _remoteIpEndPoint = new IPEndPoint(IPAddress.Loopback, port);

        _client = new UdpClient();

        _reader = new UdpReader(_server.Client, 100);
        _writer = new UdpWriter(_client.Client, 100);
    }

    [TearDown]
    public void TearDown()
    {
        _server.Dispose();
        _client.Dispose();
    }

    [Test]
    public void SendAndReceiveData([Values(10, 100)] int length)
    {
        _writer.BeginWriting();
        Span<byte> writeSpan = _writer.Reserve(length);
        Assert.That(writeSpan.Length, Is.EqualTo(length));
        for (int i = 0; i < length; i++)
        {
            writeSpan[i] = (byte)i;
        }

        NetworkWriteResult writeResult = _writer.EndWriting(_remoteIpEndPoint);

        Assert.That(writeResult.SocketError, Is.EqualTo(SocketError.Success));

        NetworkReadResult readResult = _reader.BeginReading();

        Assert.That(readResult.SocketError, Is.EqualTo(SocketError.Success));
        Assert.That(readResult.IsDisconnected, Is.EqualTo(false));
        Assert.That(readResult.RemoteIpEndPoint.Address, Is.EqualTo(_remoteIpEndPoint.Address));
        Assert.That(readResult.RemoteIpEndPoint.Port, Is.Not.EqualTo(_remoteIpEndPoint.Port));

        ReadOnlySpan<byte> readSpan = _reader.Read(length);
        _reader.EndReading();
        Assert.That(readSpan.Length, Is.EqualTo(length));

        AssertEquals(readSpan, writeSpan);
    }

    [Test]
    public void SendCompleteAndReceiveFragmentedData([Values(2, 10, 100)] int length)
    {
        _writer.BeginWriting();
        Span<byte> writeSpan = _writer.Reserve(100);
        Assert.That(writeSpan.Length, Is.EqualTo(100));
        for (int i = 0; i < length; i++)
        {
            writeSpan[i] = (byte)i;
        }

        NetworkWriteResult writeResult = _writer.EndWriting(_remoteIpEndPoint);

        Assert.That(writeResult.SocketError, Is.EqualTo(SocketError.Success));

        NetworkReadResult readResult = _reader.BeginReading();

        Assert.That(readResult.SocketError, Is.EqualTo(SocketError.Success));

        byte[] buffer = new byte[100];
        int index = 0;
        for (int i = 0; i < (100 / length); i++)
        {
            _reader.Read(length).CopyTo(buffer.AsSpan(index, length));
            index += length;
        }

        _reader.EndReading();
        AssertEquals(buffer.AsSpan(), writeSpan);
    }

    [Test]
    [TestCase(101)]
    [TestCase(50, 51)]
    [TestCase(33, 33, 35)]
    public void Overflow(params int[] arguments)
    {
        _writer.BeginWriting();
        for (int i = 0; i < (arguments.Length - 1); i++)
        {
            _ = _writer.Reserve(arguments[i]);
        }

        Assert.Throws<RpcException>(() => _ = _writer.Reserve(arguments[^1]));
    }

    [Test]
    [TestCase(11)]
    [TestCase(5, 6)]
    [TestCase(3, 3, 5)]
    public void Underflow(params int[] arguments)
    {
        _writer.BeginWriting();
        _ = _writer.Reserve(10);

        NetworkWriteResult writeResult = _writer.EndWriting(_remoteIpEndPoint);
        Assert.That(writeResult.SocketError, Is.EqualTo(SocketError.Success));

        NetworkReadResult readResult = _reader.BeginReading();
        Assert.That(readResult.SocketError, Is.EqualTo(SocketError.Success));

        for (int i = 0; i < (arguments.Length - 1); i++)
        {
            _ = _reader.Read(arguments[i]);
        }

        Assert.Throws<RpcException>(() => _ = _reader.Read(arguments[^1]));
    }

    [Test]
    public void AbortReading()
    {
        var task = Task.Run(() => _reader.BeginReading());
        Thread.Sleep(100);
        _server.Dispose();
        NetworkReadResult readResult = task.GetAwaiter().GetResult();
        Assert.That(readResult.SocketError, Is.EqualTo(SocketError.Interrupted));
    }

    private static void AssertEquals(ReadOnlySpan<byte> one, ReadOnlySpan<byte> two)
    {
        Assert.That(one.Length, Is.EqualTo(two.Length));
        for (int i = 0; i < one.Length; i++)
        {
            Assert.That(one[i], Is.EqualTo(two[i]));
        }
    }
}
