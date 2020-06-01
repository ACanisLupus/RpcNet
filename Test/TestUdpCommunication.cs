namespace RpcNet.Test
{
    using System;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using NUnit.Framework;
    using RpcNet.Internal;

    class TestUdpCommunication
    {
        private UdpBufferReader reader;
        private UdpBufferWriter writer;
        private UdpClient server;
        private UdpClient client;
        private IPEndPoint remoteIpEndPoint;

        [SetUp]
        public void SetUp()
        {
            this.server = new UdpClient(0);
            int port = ((IPEndPoint)this.server.Client.LocalEndPoint).Port;
            this.remoteIpEndPoint = new IPEndPoint(IPAddress.Loopback, port);

            this.client = new UdpClient();

            this.reader = new UdpBufferReader(this.server.Client, 100);
            this.writer = new UdpBufferWriter(this.client.Client, 100);
        }

        [TearDown]
        public void TearDown()
        {
            this.server.Dispose();
            this.client.Dispose();
        }

        [Test]
        [TestCase(0)]
        [TestCase(10)]
        [TestCase(100)]
        public void SendAndReceiveData(int length)
        {
            this.writer.BeginWriting();
            Span<byte> writeSpan = this.writer.Reserve(length);
            Assert.That(writeSpan.Length, Is.EqualTo(length));
            for (int i = 0; i < length; i++)
            {
                writeSpan[i] = (byte)i;
            }

            SocketResult socketResult = this.writer.EndWriting(this.remoteIpEndPoint);
            Assert.That(socketResult.SocketError, Is.EqualTo(SocketError.Success));
            Assert.That(socketResult.BytesLength, Is.EqualTo(length));

            this.reader.BeginReading(out IPEndPoint writerIpEndPoint);
            Assert.That(writerIpEndPoint.Address, Is.EqualTo(this.remoteIpEndPoint.Address));
            Assert.That(writerIpEndPoint.Port, Is.Not.EqualTo(this.remoteIpEndPoint.Port));
            Assert.That(writerIpEndPoint.Port, Is.GreaterThanOrEqualTo(49152));

            ReadOnlySpan<byte> readSpan = this.reader.Read(length);
            Assert.That(readSpan.Length, Is.EqualTo(length));

            AssertEquals(readSpan, writeSpan);
        }

        [Test]
        [TestCase(2)]
        [TestCase(10)]
        [TestCase(100)]
        public void SendCompleteAndReceiveFragmentedData(int length)
        {
            this.writer.BeginWriting();
            Span<byte> writeSpan = this.writer.Reserve(100);
            Assert.That(writeSpan.Length, Is.EqualTo(100));
            for (int i = 0; i < length; i++)
            {
                writeSpan[i] = (byte)i;
            }

            SocketResult socketResult = this.writer.EndWriting(this.remoteIpEndPoint);
            Assert.That(socketResult.SocketError, Is.EqualTo(SocketError.Success));
            Assert.That(socketResult.BytesLength, Is.EqualTo(100));

            this.reader.BeginReading(out _);
            byte[] buffer = new byte[100];
            int index = 0;
            for (int i = 0; i < 100 / length; i++)
            {
                this.reader.Read(length).CopyTo(buffer.AsSpan(index, length));
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
            this.writer.BeginWriting();
            for (int i = 0; i < arguments.Length - 1; i++)
            {
                this.writer.Reserve(arguments[i]);
            }

            Assert.Throws<RpcException>(() => this.writer.Reserve(arguments[arguments.Length - 1]));
        }

        [Test]
        [TestCase(11)]
        [TestCase(5, 6)]
        [TestCase(3, 3, 5)]
        public void Underflow(params int[] arguments)
        {
            this.writer.BeginWriting();
            this.writer.Reserve(10);

            SocketResult socketResult = this.writer.EndWriting(this.remoteIpEndPoint);
            Assert.That(socketResult.SocketError, Is.EqualTo(SocketError.Success));
            Assert.That(socketResult.BytesLength, Is.EqualTo(10));

            this.reader.BeginReading(out _);
            for (int i = 0; i < arguments.Length - 1; i++)
            {
                this.reader.Read(arguments[i]);
            }

            Assert.Throws<RpcException>(() => this.reader.Read(arguments[arguments.Length - 1]));
        }

        [Test]
        [TestCase(100)]
        [TestCase(200)]
        [TestCase(500)]
        public void TimeoutReader(int timeout)
        {
            this.server.Client.ReceiveTimeout = timeout;
            Assert.Throws<SocketException>(() => this.reader.BeginReading(out _));
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
