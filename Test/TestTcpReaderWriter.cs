// Copyright by Artur Wolf

namespace Test;

using System.Net;
using System.Net.Sockets;
using NUnit.Framework;
using RpcNet.Internal;

[TestFixture]
internal sealed class TestTcpReaderWriter
{
    private TcpReader? _reader;
    private TcpClient? _readerTcpClient;
    private TcpWriter? _writer;
    private TcpClient? _writerTcpClient;

    private TcpReader Reader => _reader ?? throw new InvalidOperationException("TCP reader is not initialized.");
    private TcpWriter Writer => _writer ?? throw new InvalidOperationException("TCP writer is not initialized.");
    private TcpClient WriterTcpClient => _writerTcpClient ?? throw new InvalidOperationException("Writer TPC client is not initialized.");

    [SetUp]
    public async ValueTask SetUp()
    {
        CancellationToken ct = TestContext.CurrentContext.CancellationToken;

        IPAddress ipAddress = IPAddress.Loopback;

        TcpListener listener = new(ipAddress, 0);
        listener.Start();

        IPEndPoint? localIpEndPoint = listener.Server.LocalEndPoint as IPEndPoint;
        int port = localIpEndPoint?.Port ?? throw new InvalidOperationException("Could not find local end point.");
        _readerTcpClient = new TcpClient();
        Task<TcpClient> task = Task.Run(async () => await listener.AcceptTcpClientAsync(ct), ct);
        await _readerTcpClient.ConnectAsync(ipAddress, port, ct);
        _writerTcpClient = await task;
        _reader = new TcpReader(_readerTcpClient.Client);
        _writer = new TcpWriter(_writerTcpClient.Client);

        listener.Stop();
    }

    [TearDown]
    public void TearDown()
    {
        _readerTcpClient?.Dispose();
        _writerTcpClient?.Dispose();
    }

    [Test]
    public void ReadAndWriteMultipleFragments()
    {
        CancellationToken ct = TestContext.CurrentContext.CancellationToken;

        XdrReader xdrReader = new(Reader);
        XdrWriter xdrWriter = new(Writer);

        byte[] value = TestXdr.GenerateByteTestData(17);

        Writer.BeginWriting();
        xdrWriter.WriteOpaque(value);
        xdrWriter.Write(42);

        Assert.DoesNotThrowAsync(async () => await Writer.EndWritingAsync(new IPEndPoint(0, 0), ct));

        Assert.DoesNotThrowAsync(async () => await Reader.BeginReadingAsync(ct));

        Assert.That(xdrReader.ReadOpaque(), Is.EqualTo(value));
        Assert.That(xdrReader.ReadInt32(), Is.EqualTo(42));
        Reader.EndReading();
    }

    [Test]
    public async ValueTask ReadAndWriteMultipleFragmentsThreaded()
    {
        CancellationToken ct = TestContext.CurrentContext.CancellationToken;

        WriterTcpClient.Client.SendBufferSize = 1;

        XdrReader xdrReader = new(Reader);
        XdrWriter xdrWriter = new(Writer);

        byte[] value = TestXdr.GenerateByteTestData(17);

        Task task = Task.Run(
            () =>
            {
                Assert.DoesNotThrowAsync(async () => await Reader.BeginReadingAsync(ct));

                Assert.That(xdrReader.ReadOpaque(), Is.EqualTo(value));
                Assert.That(xdrReader.ReadInt32(), Is.EqualTo(42));
                Reader.EndReading();
            },
            ct);

        Writer.BeginWriting();
        xdrWriter.WriteOpaque(value);
        xdrWriter.Write(42);
        Assert.DoesNotThrowAsync(async () => await Writer.EndWritingAsync(new IPEndPoint(0, 0), ct));

        await task;
    }
}
