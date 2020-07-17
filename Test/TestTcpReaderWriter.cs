namespace RpcNet.Test
{
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using RpcNet.Internal;

    internal class TestTcpReaderWriter
    {
        private TcpListener listener;
        private TcpReader reader;
        private TcpClient readerTcpClient;
        private TcpWriter writer;
        private TcpClient writerTcpClient;

        [SetUp]
        public void SetUp()
        {
            this.listener = new TcpListener(IPAddress.Loopback, 0);
            this.listener.Start();
            int port = ((IPEndPoint)this.listener.Server.LocalEndPoint).Port;
            this.readerTcpClient = new TcpClient();
            Task task = Task.Run(() => this.writerTcpClient = this.listener.AcceptTcpClient());
            this.readerTcpClient.Connect(IPAddress.Loopback, port);
            task.GetAwaiter().GetResult();
            this.reader = new TcpReader(this.readerTcpClient.Client);
            this.writer = new TcpWriter(this.writerTcpClient.Client);
            this.listener.Stop();
        }

        [TearDown]
        public void TearDown()
        {
            this.readerTcpClient.Dispose();
            this.writerTcpClient.Dispose();
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
            this.reader = new TcpReader(this.readerTcpClient.Client, maxReadLength);
            this.writer = new TcpWriter(this.writerTcpClient.Client, maxReserveLength);

            var xdrReader = new XdrReader(this.reader);
            var xdrWriter = new XdrWriter(this.writer);

            byte[] value = TestXdr.GenerateByteTestData(17);

            this.writer.BeginWriting();
            xdrWriter.WriteVariableLengthOpaque(value);
            xdrWriter.Write(42);
            NetworkWriteResult writeResult = this.writer.EndWriting();
            Assert.That(writeResult.SocketError, Is.EqualTo(SocketError.Success));

            NetworkReadResult readResult = this.reader.BeginReading();
            Assert.That(readResult.SocketError, Is.EqualTo(SocketError.Success));
            Assert.That(xdrReader.ReadOpaque(), Is.EqualTo(value));
            Assert.That(xdrReader.ReadInt(), Is.EqualTo(42));
            this.reader.EndReading();
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
            this.reader = new TcpReader(this.readerTcpClient.Client, maxReadLength);
            this.writer = new TcpWriter(this.writerTcpClient.Client, maxReserveLength);

            this.writerTcpClient.Client.SendBufferSize = 1;

            var xdrReader = new XdrReader(this.reader);
            var xdrWriter = new XdrWriter(this.writer);

            byte[] value = TestXdr.GenerateByteTestData(17);

            Task task = Task.Run(
                () =>
                {
                    NetworkReadResult readResult = this.reader.BeginReading();
                    Assert.That(readResult.SocketError, Is.EqualTo(SocketError.Success));
                    Assert.That(xdrReader.ReadOpaque(), Is.EqualTo(value));
                    Assert.That(xdrReader.ReadInt(), Is.EqualTo(42));
                    this.reader.EndReading();
                });

            this.writer.BeginWriting();
            xdrWriter.WriteVariableLengthOpaque(value);
            xdrWriter.Write(42);
            this.writer.EndWriting();

            task.Wait();
        }
    }
}
