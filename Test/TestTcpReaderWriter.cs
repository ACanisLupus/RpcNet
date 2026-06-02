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

    private TcpClient ReaderTcpClient => _readerTcpClient ?? throw new InvalidOperationException("Reader TPC client is not initialized.");
    private TcpClient WriterTcpClient => _writerTcpClient ?? throw new InvalidOperationException("Writer TPC client is not initialized.");

    [SetUp]
    public void SetUp()
    {
        IPAddress ipAddress = IPAddress.Loopback;

        TcpListener listener = new(ipAddress, 0);
        listener.Start();

        IPEndPoint? localIpEndPoint = listener.Server.LocalEndPoint as IPEndPoint;
        int port = localIpEndPoint?.Port ?? throw new InvalidOperationException("Could not find local end point.");
        _readerTcpClient = new TcpClient();
        Task<TcpClient> task = Task.Run(listener.AcceptTcpClient);
        _readerTcpClient.Connect(ipAddress, port);
        _writerTcpClient = task.GetAwaiter().GetResult();
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
    [TestCase(32, 32)]
    [TestCase(32, 8)]
    [TestCase(8, 32)]
    [TestCase(8, 8)]
    [TestCase(12, 8)]
    [TestCase(8, 12)]
    public void ReadAndWriteMultipleFragments(int maxReadLength, int maxReserveLength)
    {
        _reader = new TcpReader(ReaderTcpClient.Client, maxReadLength);
        _writer = new TcpWriter(WriterTcpClient.Client, maxReserveLength);

        XdrReader xdrReader = new(_reader);
        XdrWriter xdrWriter = new(_writer);

        byte[] value = TestXdr.GenerateByteTestData(17);

        _writer.BeginWriting();
        xdrWriter.WriteOpaque(value);
        xdrWriter.Write(42);

        Assert.DoesNotThrow(() => _writer.EndWriting(new IPEndPoint(0, 0)));

        Assert.DoesNotThrow(() => _reader.BeginReading());

        Assert.That(xdrReader.ReadOpaque(), Is.EqualTo(value));
        Assert.That(xdrReader.ReadInt32(), Is.EqualTo(42));
        _reader.EndReading();
    }

    [Test]
    [TestCase(32, 32)]
    [TestCase(32, 8)]
    [TestCase(8, 32)]
    [TestCase(8, 8)]
    [TestCase(12, 8)]
    [TestCase(8, 12)]
    public void ReadAndWriteMultipleFragmentsThreaded(int maxReadLength, int maxReserveLength)
    {
        _reader = new TcpReader(ReaderTcpClient.Client, maxReadLength);
        _writer = new TcpWriter(WriterTcpClient.Client, maxReserveLength);

        WriterTcpClient.Client.SendBufferSize = 1;

        XdrReader xdrReader = new(_reader);
        XdrWriter xdrWriter = new(_writer);

        byte[] value = TestXdr.GenerateByteTestData(17);

        Task task = Task.Run(
            () =>
            {
                Assert.DoesNotThrow(() => _reader.BeginReading());

                Assert.That(xdrReader.ReadOpaque(), Is.EqualTo(value));
                Assert.That(xdrReader.ReadInt32(), Is.EqualTo(42));
                _reader.EndReading();
            });

        _writer.BeginWriting();
        xdrWriter.WriteOpaque(value);
        xdrWriter.Write(42);
        _writer.EndWriting(new IPEndPoint(0, 0));

        task.Wait();
    }
}
