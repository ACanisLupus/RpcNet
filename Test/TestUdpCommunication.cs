namespace RpcNet.Test
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using RpcNet.Internal;

    class TestUdpCommunication
    {
        private UdpServiceReader serviceReader;
        private UdpServiceWriter serviceWriter;
        private UdpClient server;
        private UdpClient client;
        private IPEndPoint remoteIpEndPoint;
        private Channel<UdpResult> readChannel;
        private Channel<UdpResult> writeChannel;

        [SetUp]
        public void SetUp()
        {
            this.server = new UdpClient(0);
            int port = ((IPEndPoint)this.server.Client.LocalEndPoint).Port;
            this.remoteIpEndPoint = new IPEndPoint(IPAddress.Loopback, port);

            this.client = new UdpClient();

            this.serviceReader = new UdpServiceReader(this.server.Client, 100);
            this.serviceReader.Completed += udpResult => this.readChannel.Send(udpResult);
            this.serviceWriter = new UdpServiceWriter(this.client.Client, 100);
            this.serviceWriter.Completed += udpResult => this.writeChannel.Send(udpResult);

            this.readChannel = new Channel<UdpResult>();
            this.writeChannel = new Channel<UdpResult>();
        }

        [TearDown]
        public void TearDown()
        {
            this.server.Dispose();
            this.client.Dispose();
            this.readChannel.Close();
            this.writeChannel.Close();
        }

        [Test]
        [TestCase(0)]
        [TestCase(10)]
        [TestCase(100)]
        public void SendAndReceiveData(int length)
        {
            this.serviceWriter.BeginWriting();
            Span<byte> writeSpan = this.serviceWriter.Reserve(length);
            Assert.That(writeSpan.Length, Is.EqualTo(length));
            for (int i = 0; i < length; i++)
            {
                writeSpan[i] = (byte)i;
            }

            this.serviceWriter.EndWriting(this.remoteIpEndPoint);
            Assert.That(this.writeChannel.Receive(out UdpResult udpResult));
            Assert.That(udpResult.SocketError, Is.EqualTo(SocketError.Success));
            Assert.That(udpResult.BytesLength, Is.EqualTo(length));

            this.serviceReader.BeginReading();
            this.serviceReader.EndReading();
            Assert.That(this.readChannel.Receive(out udpResult));
            Assert.That(udpResult.SocketError, Is.EqualTo(SocketError.Success));
            Assert.That(udpResult.BytesLength, Is.EqualTo(length));
            Assert.That(udpResult.IpEndPoint.Address, Is.EqualTo(this.remoteIpEndPoint.Address));
            Assert.That(udpResult.IpEndPoint.Port, Is.Not.EqualTo(this.remoteIpEndPoint.Port));
            Assert.That(udpResult.IpEndPoint.Port, Is.GreaterThanOrEqualTo(49152));

            ReadOnlySpan<byte> readSpan = this.serviceReader.Read(length);
            Assert.That(readSpan.Length, Is.EqualTo(length));

            AssertEquals(readSpan, writeSpan);
        }

        [Test]
        [TestCase(2)]
        [TestCase(10)]
        [TestCase(100)]
        public void SendCompleteAndReceiveFragmentedData(int length)
        {
            this.serviceWriter.BeginWriting();
            Span<byte> writeSpan = this.serviceWriter.Reserve(100);
            Assert.That(writeSpan.Length, Is.EqualTo(100));
            for (int i = 0; i < length; i++)
            {
                writeSpan[i] = (byte)i;
            }

            this.serviceWriter.EndWriting(this.remoteIpEndPoint);
            Assert.That(this.writeChannel.Receive(out UdpResult udpResult));
            Assert.That(udpResult.SocketError, Is.EqualTo(SocketError.Success));
            Assert.That(udpResult.BytesLength, Is.EqualTo(100));

            this.serviceReader.BeginReading();
            this.serviceReader.EndReading();
            Assert.That(this.readChannel.Receive(out udpResult));
            byte[] buffer = new byte[100];
            int index = 0;
            for (int i = 0; i < 100 / length; i++)
            {
                this.serviceReader.Read(length).CopyTo(buffer.AsSpan(index, length));
                index += length;
            }

            AssertEquals(buffer.AsSpan(), writeSpan);
        }

        [Test]
        [TestCase(101)]
        [TestCase(50, 51)]
        [TestCase(33, 33, 35)]
        public void Overflow(params int[] arguments)
        {
            this.serviceWriter.BeginWriting();
            for (int i = 0; i < arguments.Length - 1; i++)
            {
                this.serviceWriter.Reserve(arguments[i]);
            }

            Assert.Throws<RpcException>(() => this.serviceWriter.Reserve(arguments[arguments.Length - 1]));
        }

        [Test]
        [TestCase(11)]
        [TestCase(5, 6)]
        [TestCase(3, 3, 5)]
        public void Underflow(params int[] arguments)
        {
            this.serviceWriter.BeginWriting();
            this.serviceWriter.Reserve(10);

            this.serviceWriter.EndWriting(this.remoteIpEndPoint);
            Assert.That(this.writeChannel.Receive(out UdpResult udpResult));
            Assert.That(udpResult.SocketError, Is.EqualTo(SocketError.Success));
            Assert.That(udpResult.BytesLength, Is.EqualTo(10));

            this.serviceReader.BeginReading();
            this.serviceReader.EndReading();
            Assert.That(this.readChannel.Receive(out udpResult));
            for (int i = 0; i < arguments.Length - 1; i++)
            {
                this.serviceReader.Read(arguments[i]);
            }

            Assert.Throws<RpcException>(() => this.serviceReader.Read(arguments[arguments.Length - 1]));
        }

        [Test]
        public void AbortReading()
        {
            var task = Task.Run(() =>
            {
                this.serviceReader.BeginReading();
                this.serviceReader.EndReading();
                Assert.That(this.readChannel.Receive(out UdpResult udpResult));
                Assert.That(udpResult.SocketError, Is.EqualTo(SocketError.OperationAborted));
            });
            Thread.Sleep(100);
            this.server.Dispose();
            this.serviceReader.Dispose();
            task.Wait();
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
}
