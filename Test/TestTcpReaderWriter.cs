// Copyright by Artur Wolf

namespace Test;

using System.Net;
using System.Net.Sockets;
using NUnit.Framework;
using RpcNet.Internal;

[TestFixture]
internal sealed class TestTcpReaderWriter
{
    private TcpReader _reader;
    private TcpClient _readerTcpClient;
    private TcpWriter _writer;
    private TcpClient _writerTcpClient;

    [SetUp]
    public void SetUp()
    {
        IPAddress ipAddress = IPAddress.Loopback;

        TcpListener listener = new(ipAddress, 0);
        listener.Start();

        int port = ((IPEndPoint)listener.Server.LocalEndPoint)?.Port ?? throw new InvalidOperationException("Could not find local end point.");
        _readerTcpClient = new TcpClient();
        Task task = Task.Run(() => _writerTcpClient = listener.AcceptTcpClient());
        _readerTcpClient.Connect(ipAddress, port);
        task.GetAwaiter().GetResult();
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
        _reader = new TcpReader(_readerTcpClient.Client, maxReadLength);
        _writer = new TcpWriter(_writerTcpClient.Client, maxReserveLength);

        var xdrReader = new XdrReader(_reader);
        var xdrWriter = new XdrWriter(_writer);

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
        _reader = new TcpReader(_readerTcpClient.Client, maxReadLength);
        _writer = new TcpWriter(_writerTcpClient.Client, maxReserveLength);

        _writerTcpClient.Client.SendBufferSize = 1;

        var xdrReader = new XdrReader(_reader);
        var xdrWriter = new XdrWriter(_writer);

        byte[] value = TestXdr.GenerateByteTestData(17);

        var task = Task.Run(
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
