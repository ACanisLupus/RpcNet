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
    private UdpClient? _client;
    private UdpClient? _server;

    private UdpReader Reader { get => field ?? throw new InvalidOperationException("UDP reader is not initialized."); set; }
    private IPEndPoint RemoteIpEndPoint { get => field ?? throw new InvalidOperationException("Remote IP end point is not initialized."); set; }
    private UdpClient Server => _server ?? throw new InvalidOperationException("UDP server is not initialized.");
    private UdpWriter Writer { get => field ?? throw new InvalidOperationException("UDP writer is not initialized."); set; }

    [SetUp]
    public void SetUp()
    {
        IPAddress iPAddress = IPAddress.Loopback;

        _server = new UdpClient(0, iPAddress.AddressFamily);

        IPEndPoint? localIpEndPoint = _server.Client.LocalEndPoint as IPEndPoint;
        int port = localIpEndPoint?.Port ?? throw new InvalidOperationException("Could not find local end point.");
        RemoteIpEndPoint = new IPEndPoint(iPAddress, port);

        _client = new UdpClient(iPAddress.AddressFamily);

        Reader = new UdpReader(_server.Client);
        Writer = new UdpWriter(_client.Client);
    }

    [TearDown]
    public void TearDown()
    {
        _server?.Dispose();
        _client?.Dispose();
    }

    [Test]
    public async ValueTask SendAndReceiveData([Values(10, 100)] int length)
    {
        CancellationToken ct = TestContext.CurrentContext.CancellationToken;

        Writer.BeginWriting();
        Span<byte> writeSpan = Writer.Reserve(length);
        Assert.That(writeSpan.Length, Is.EqualTo(length));
        for (int i = 0; i < length; i++)
        {
            writeSpan[i] = (byte)i;
        }

        byte[] writtenData = writeSpan.ToArray();

        Assert.DoesNotThrowAsync(async () => await Writer.EndWritingAsync(RemoteIpEndPoint, ct));

        IPEndPoint remoteIdEndPoint = (IPEndPoint)await Reader.BeginReadingAsync(ct);

        Assert.That(remoteIdEndPoint.Address, Is.EqualTo(RemoteIpEndPoint.Address));
        Assert.That(remoteIdEndPoint.Port, Is.Not.EqualTo(RemoteIpEndPoint.Port));

        ReadOnlySpan<byte> readSpan = Reader.Read(length);
        Reader.EndReading();
        Assert.That(readSpan.Length, Is.EqualTo(length));

        AssertEquals(readSpan, writtenData);
    }

    [Test]
    public void SendCompleteAndReceiveFragmentedData([Values(2, 10, 100)] int length)
    {
        CancellationToken ct = TestContext.CurrentContext.CancellationToken;

        Writer.BeginWriting();
        Span<byte> writeSpan = Writer.Reserve(100);
        Assert.That(writeSpan.Length, Is.EqualTo(100));
        for (int i = 0; i < length; i++)
        {
            writeSpan[i] = (byte)i;
        }

        Assert.DoesNotThrowAsync(async () => await Writer.EndWritingAsync(RemoteIpEndPoint, ct));

        Assert.DoesNotThrowAsync(async () => await Reader.BeginReadingAsync(ct));

        byte[] buffer = new byte[100];
        int index = 0;
        for (int i = 0; i < (100 / length); i++)
        {
            Reader.Read(length).CopyTo(buffer.AsSpan(index, length));
            index += length;
        }

        Reader.EndReading();
        AssertEquals(buffer.AsSpan(), writeSpan);
    }

    [Test]
    [TestCase(11)]
    [TestCase(5, 6)]
    [TestCase(3, 3, 5)]
    public void Underflow(params int[] arguments)
    {
        CancellationToken ct = TestContext.CurrentContext.CancellationToken;

        Writer.BeginWriting();
        _ = Writer.Reserve(10);

        Assert.DoesNotThrowAsync(async () => await Writer.EndWritingAsync(RemoteIpEndPoint, ct));

        Assert.DoesNotThrowAsync(async () => await Reader.BeginReadingAsync(ct));

        for (int i = 0; i < (arguments.Length - 1); i++)
        {
            _ = Reader.Read(arguments[i]);
        }

        _ = Assert.Throws<RpcException>(() => Reader.Read(arguments[^1]));
    }

    [Test]
    public async ValueTask AbortReading()
    {
        CancellationToken ct = TestContext.CurrentContext.CancellationToken;

        ValueTask<EndPoint> task = await Task.Run(() => Reader.BeginReadingAsync(ct), ct);
        await Task.Delay(100, ct).ConfigureAwait(false);
        Server.Dispose();
        RpcException? e = Assert.ThrowsAsync<RpcException>(async () => await task);
        Assert.That(e?.Message, Is.EqualTo("Could not receive data from UDP socket. Socket error code: OperationAborted."));
    }

    [Test]
    public void ReadingTimesOutWhenNoDataIsReceived()
    {
        CancellationToken ct = TestContext.CurrentContext.CancellationToken;

        TimeSpan timeout = TimeSpan.FromMilliseconds(200);
        Reader.Timeout = timeout;

        // No data is ever sent, so the asynchronous reception must be aborted by the configured timeout
        // instead of blocking forever (Socket.ReceiveTimeout does not apply to ReceiveFromAsync).
        RpcException? e = Assert.ThrowsAsync<RpcException>(async () => await Reader.BeginReadingAsync(ct));
        Assert.That(e?.Message, Is.EqualTo($"The operation did not complete within the configured timeout of {timeout}."));
    }

    [Test]
    public async ValueTask ReadingDoesNotTimeOutWhenDataIsReceivedInTime()
    {
        CancellationToken ct = TestContext.CurrentContext.CancellationToken;

        Reader.Timeout = TimeSpan.FromSeconds(30);

        Writer.BeginWriting();
        Span<byte> writeSpan = Writer.Reserve(8);
        for (int i = 0; i < writeSpan.Length; i++)
        {
            writeSpan[i] = (byte)i;
        }

        byte[] writtenData = writeSpan.ToArray();

        Assert.DoesNotThrowAsync(async () => await Writer.EndWritingAsync(RemoteIpEndPoint, ct));

        _ = await Reader.BeginReadingAsync(ct);

        ReadOnlySpan<byte> readSpan = Reader.Read(writtenData.Length);
        Reader.EndReading();

        AssertEquals(readSpan, writtenData);
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
