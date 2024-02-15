// Copyright by Artur Wolf

namespace Test;

using System.Net;
using System.Net.Sockets;
using NUnit.Framework;
using RpcNet;
using RpcNet.Internal;

[TestFixture]
internal sealed class TestUdpReaderWriter
{
    private UdpClient _client = null!;
    private UdpReader _reader = null!;
    private IPEndPoint _remoteIpEndPoint = null!;
    private UdpClient _server = null!;
    private UdpWriter _writer = null!;

    [SetUp]
    public void SetUp()
    {
        IPAddress iPAddress = IPAddress.Loopback;

        _server = new UdpClient(0, iPAddress.AddressFamily);

        var localIpEndPoint = _server.Client.LocalEndPoint as IPEndPoint;
        int port = localIpEndPoint?.Port ?? throw new InvalidOperationException("Could not find local end point.");
        _remoteIpEndPoint = new IPEndPoint(iPAddress, port);

        _client = new UdpClient(iPAddress.AddressFamily);

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

        Assert.DoesNotThrow(() => _writer.EndWriting(_remoteIpEndPoint));

        var remoteIdEndPoint = (IPEndPoint)_reader.BeginReading();

        Assert.That(remoteIdEndPoint.Address, Is.EqualTo(_remoteIpEndPoint.Address));
        Assert.That(remoteIdEndPoint.Port, Is.Not.EqualTo(_remoteIpEndPoint.Port));

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

        Assert.DoesNotThrow(() => _writer.EndWriting(_remoteIpEndPoint));

        Assert.DoesNotThrow(() => _reader.BeginReading());

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

        _ = Assert.Throws<RpcException>(() => _ = _writer.Reserve(arguments[^1]));
    }

    [Test]
    [TestCase(11)]
    [TestCase(5, 6)]
    [TestCase(3, 3, 5)]
    public void Underflow(params int[] arguments)
    {
        _writer.BeginWriting();
        _ = _writer.Reserve(10);

        Assert.DoesNotThrow(() => _writer.EndWriting(_remoteIpEndPoint));

        Assert.DoesNotThrow(() => _reader.BeginReading());

        for (int i = 0; i < (arguments.Length - 1); i++)
        {
            _ = _reader.Read(arguments[i]);
        }

        _ = Assert.Throws<RpcException>(() => _ = _reader.Read(arguments[^1]));
    }

    [Test]
    public void AbortReading()
    {
        var task = Task.Run(() => _reader.BeginReading());
        Thread.Sleep(100);
        _server.Dispose();
        RpcException? e = Assert.Throws<RpcException>(() => task.GetAwaiter().GetResult());
        Assert.That(e?.Message, Is.EqualTo("Could not receive data from UDP socket. Socket error code: Interrupted."));
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
