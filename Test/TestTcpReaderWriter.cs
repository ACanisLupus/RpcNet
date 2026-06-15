// Copyright by Artur Wolf

namespace Test;

using System.Net;
using System.Net.Sockets;
using NUnit.Framework;
using RpcNet;
using RpcNet.Internal;

[TestFixture]
[TestFixtureSource(typeof(Utilities), nameof(Utilities.GetIpAddresses))]
internal sealed class TestTcpReaderWriter(IPAddress ipAddress)
{
    private TcpReader? _reader;
    private TcpClient? _readerTcpClient;
    private TcpWriter? _writer;
    private TcpClient? _writerTcpClient;

    private TcpReader Reader => _reader ?? throw new InvalidOperationException("TCP reader is not initialized.");
    private TcpWriter Writer => _writer ?? throw new InvalidOperationException("TCP writer is not initialized.");
    private TcpClient ReaderTcpClient => _readerTcpClient ?? throw new InvalidOperationException("Reader TCP client is not initialized.");
    private TcpClient WriterTcpClient => _writerTcpClient ?? throw new InvalidOperationException("Writer TPC client is not initialized.");

    [SetUp]
    public async ValueTask SetUp()
    {
        CancellationToken ct = TestContext.CurrentContext.CancellationToken;

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

    [Test]
    public void ReadingTimesOutWhenNoDataIsReceived()
    {
        CancellationToken ct = TestContext.CurrentContext.CancellationToken;

        TimeSpan timeout = TimeSpan.FromMilliseconds(200);
        Reader.Timeout = timeout;

        // The connection stays open but no data is sent, so the asynchronous reception must be cancelled by the
        // configured timeout instead of blocking forever (Socket.ReceiveTimeout does not apply to ReceiveAsync).
        RpcException? e = Assert.ThrowsAsync<RpcException>(async () => await Reader.BeginReadingAsync(ct));
        Assert.That(e?.Message, Is.EqualTo($"The operation did not complete within the configured timeout of {timeout}."));
    }

    [Test]
    public async ValueTask ReadAssemblesMultipleXdrFragments()
    {
        CancellationToken ct = TestContext.CurrentContext.CancellationToken;

        // The first fragment is not the last one (highest bit clear); the second one carries the
        // last-fragment flag (highest bit set). The reader must concatenate both payloads into one record.
        byte[] data =
        [
            0x00, 0x00, 0x00, 0x04, // fragment 1 header: length 4, not last
            0x00, 0x00, 0x00, 0x2a, // fragment 1 payload: 42
            0x80, 0x00, 0x00, 0x04, // fragment 2 header: length 4, last
            0x00, 0x00, 0x00, 0x63 // fragment 2 payload: 99
        ];

        await WriterTcpClient.GetStream().WriteAsync(data, ct);

        XdrReader xdrReader = new(Reader);
        EndPoint endPoint = await Reader.BeginReadingAsync(ct);

        Assert.That(endPoint, Is.Not.Null);
        Assert.That(xdrReader.ReadInt32(), Is.EqualTo(42));
        Assert.That(xdrReader.ReadInt32(), Is.EqualTo(99));
        Reader.EndReading();
    }

    [Test]
    public async ValueTask ReadThrowsWhenStreamIsNotXdr()
    {
        CancellationToken ct = TestContext.CurrentContext.CancellationToken;

        // A fragment length that is not a multiple of four cannot be a valid XDR stream.
        byte[] header = [0x80, 0x00, 0x00, 0x03];
        await WriterTcpClient.GetStream().WriteAsync(header, ct);

        RpcException? e = Assert.ThrowsAsync<RpcException>(async () => await Reader.BeginReadingAsync(ct));
        Assert.That(e?.Message, Is.EqualTo("This is not an XDR stream."));
    }

    [Test]
    public async ValueTask ReadThrowsWhenReadingBeyondTheRecord()
    {
        CancellationToken ct = TestContext.CurrentContext.CancellationToken;

        // A single last fragment carrying exactly one 4-byte integer.
        byte[] data = [0x80, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x2a];
        await WriterTcpClient.GetStream().WriteAsync(data, ct);

        XdrReader xdrReader = new(Reader);
        _ = await Reader.BeginReadingAsync(ct);

        Assert.That(xdrReader.ReadInt32(), Is.EqualTo(42));

        // Reading past the end of the buffered record must fail cleanly instead of returning garbage.
        RpcException? e = Assert.Throws<RpcException>(() => xdrReader.ReadInt32());
        Assert.That(e?.Message, Is.EqualTo("TCP buffer underflow."));
        Reader.EndReading();
    }

    [Test]
    public void ReadThrowsWhenPeerDisconnects()
    {
        CancellationToken ct = TestContext.CurrentContext.CancellationToken;

        // Gracefully shutting down the writer side makes the reader observe an orderly close (zero bytes).
        WriterTcpClient.Client.Shutdown(SocketShutdown.Send);

        RpcException? e = Assert.ThrowsAsync<RpcException>(async () => await Reader.BeginReadingAsync(ct));
        Assert.That(e?.Message, Does.Contain("disconnected."));
    }

    [Test]
    public async ValueTask WriterFramesPayloadAsSingleLastFragment()
    {
        CancellationToken ct = TestContext.CurrentContext.CancellationToken;

        XdrWriter xdrWriter = new(Writer);
        Writer.BeginWriting();
        xdrWriter.Write(42);
        await Writer.EndWritingAsync(new IPEndPoint(0, 0), ct);

        // Read the raw framing produced by the writer: a 4-byte record header followed by the 4-byte payload.
        byte[] raw = new byte[8];
        int read = 0;
        while (read < raw.Length)
        {
            int n = await ReaderTcpClient.GetStream().ReadAsync(raw.AsMemory(read), ct);
            Assert.That(n, Is.GreaterThan(0));
            read += n;
        }

        int header = (raw[0] << 24) | (raw[1] << 16) | (raw[2] << 8) | raw[3];

        // The highest bit marks the last fragment; the low 31 bits hold the payload length.
        Assert.That(header & unchecked((int)0x80000000), Is.Not.EqualTo(0));
        Assert.That(header & 0x7fffffff, Is.EqualTo(sizeof(int)));
        Assert.That((raw[4] << 24) | (raw[5] << 16) | (raw[6] << 8) | raw[7], Is.EqualTo(42));
    }
}
