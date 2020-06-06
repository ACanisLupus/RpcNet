using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using NUnit.Framework;
using RpcNet.Internal;

namespace RpcNet.Test
{
    class TestTcpCommunication
    {
        private TcpBufferReader reader;
        private TcpBufferWriter writer;
        private TcpListener listener;
        private TcpClient readerTcpClient;
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
            this.reader = new TcpBufferReader(this.readerTcpClient.Client);
            this.writer = new TcpBufferWriter(this.writerTcpClient.Client);
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
        public void ReserveMultipleFragmentsOpaque(int maxReadLength, int maxReserveLength)
        {
            this.reader = new TcpBufferReader(this.readerTcpClient.Client, maxReadLength);
            this.writer = new TcpBufferWriter(this.writerTcpClient.Client, maxReserveLength);

            var xdrReader = new XdrReader(this.reader);
            var xdrWriter = new XdrWriter(this.writer);

            byte[] value = TestXdr.GenerateByteTestData(17);

            writer.BeginWriting();
            xdrWriter.WriteVariableLengthOpaque(value);
            xdrWriter.Write(42);
            writer.EndWriting();

            Assert.That(reader.BeginReading(out SocketError socketError), Is.True);
            Assert.That(socketError, Is.EqualTo(SocketError.Success));
            Assert.That(xdrReader.ReadOpaque(), Is.EqualTo(value));
            Assert.That(xdrReader.ReadInt(), Is.EqualTo(42));
            reader.EndReading();
        }
    }
}
