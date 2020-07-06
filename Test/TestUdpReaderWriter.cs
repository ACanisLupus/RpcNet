namespace RpcNet.Test
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using RpcNet.Internal;

    internal class TestUdpReaderWriter
    {
        private UdpClient client;
        private Channel<NetworkReadResult> readChannel;
        private UdpReader reader;
        private IPEndPoint remoteIpEndPoint;
        private UdpClient server;
        private Channel<NetworkWriteResult> writeChannel;
        private UdpWriter writer;

        [SetUp]
        public void SetUp()
        {
            this.server = new UdpClient(0);

            int port = ((IPEndPoint)this.server.Client.LocalEndPoint).Port;
            this.remoteIpEndPoint = new IPEndPoint(IPAddress.Loopback, port);

            this.client = new UdpClient();

            this.reader = new UdpReader(this.server.Client, 100, TestLogger.Instance);
            this.reader.Completed += udpResult => this.readChannel.Send(udpResult);

            this.writer = new UdpWriter(this.client.Client, 100, TestLogger.Instance);
            this.writer.Completed += udpResult => this.writeChannel.Send(udpResult);

            this.readChannel = new Channel<NetworkReadResult>();
            this.writeChannel = new Channel<NetworkWriteResult>();
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
        public void SendAndReceiveData([Values(0, 10, 100)] int length, [Values(true, false)] bool syncWriting, [Values(true, false)] bool syncReading)
        {
            this.writer.BeginWriting();
            Span<byte> writeSpan = this.writer.Reserve(length);
            Assert.That(writeSpan.Length, Is.EqualTo(length));
            for (int i = 0; i < length; i++)
            {
                writeSpan[i] = (byte)i;
            }

            NetworkWriteResult writeResult;
            if (syncWriting)
            {
                writeResult = this.writer.EndWriting(this.remoteIpEndPoint);
            }
            else
            {
                this.writer.EndWritingAsync(this.remoteIpEndPoint);
                Assert.That(this.writeChannel.Receive(out writeResult));
            }

            Assert.That(writeResult.SocketError, Is.EqualTo(SocketError.Success));

            NetworkReadResult readResult;
            if (syncReading)
            {
                readResult = this.reader.BeginReading();
            }
            else
            {
                this.reader.BeginReadingAsync();
                Assert.That(this.readChannel.Receive(out readResult));
            }

            Assert.That(readResult.SocketError, Is.EqualTo(SocketError.Success));
            Assert.That(readResult.RemoteIpEndPoint.Address, Is.EqualTo(this.remoteIpEndPoint.Address));
            Assert.That(readResult.RemoteIpEndPoint.Port, Is.Not.EqualTo(this.remoteIpEndPoint.Port));
            Assert.That(readResult.RemoteIpEndPoint.Port, Is.GreaterThanOrEqualTo(49152));

            ReadOnlySpan<byte> readSpan = this.reader.Read(length);
            this.reader.EndReading();
            Assert.That(readSpan.Length, Is.EqualTo(length));

            AssertEquals(readSpan, writeSpan);
        }

        [Test]
        public void SendCompleteAndReceiveFragmentedData(
            [Values(2, 10, 100)] int length,
            [Values(true, false)] bool syncWriting,
            [Values(true, false)] bool syncReading)
        {
            this.writer.BeginWriting();
            Span<byte> writeSpan = this.writer.Reserve(100);
            Assert.That(writeSpan.Length, Is.EqualTo(100));
            for (int i = 0; i < length; i++)
            {
                writeSpan[i] = (byte)i;
            }

            NetworkWriteResult writeResult;
            if (syncWriting)
            {
                writeResult = this.writer.EndWriting(this.remoteIpEndPoint);
            }
            else
            {
                this.writer.EndWritingAsync(this.remoteIpEndPoint);
                Assert.That(this.writeChannel.Receive(out writeResult));
            }

            Assert.That(writeResult.SocketError, Is.EqualTo(SocketError.Success));

            NetworkReadResult readResult;
            if (syncReading)
            {
                readResult = this.reader.BeginReading();
            }
            else
            {
                this.reader.BeginReadingAsync();
                Assert.That(this.readChannel.Receive(out readResult));
            }

            Assert.That(readResult.SocketError, Is.EqualTo(SocketError.Success));

            var buffer = new byte[100];
            int index = 0;
            for (int i = 0; i < (100 / length); i++)
            {
                this.reader.Read(length).CopyTo(buffer.AsSpan(index, length));
                index += length;
            }

            this.reader.EndReading();
            AssertEquals(buffer.AsSpan(), writeSpan);
        }

        [Test]
        [TestCase(101)]
        [TestCase(50, 51)]
        [TestCase(33, 33, 35)]
        public void Overflow(params int[] arguments)
        {
            this.writer.BeginWriting();
            for (int i = 0; i < (arguments.Length - 1); i++)
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

            NetworkWriteResult writeResult = this.writer.EndWriting(this.remoteIpEndPoint);
            Assert.That(writeResult.SocketError, Is.EqualTo(SocketError.Success));

            NetworkReadResult readResult = this.reader.BeginReading();
            Assert.That(readResult.SocketError, Is.EqualTo(SocketError.Success));

            for (int i = 0; i < (arguments.Length - 1); i++)
            {
                this.reader.Read(arguments[i]);
            }

            Assert.Throws<RpcException>(() => this.reader.Read(arguments[arguments.Length - 1]));
        }

        [Test]
        public void AbortReading([Values(true, false)] bool syncReading)
        {
            var task = Task.Run(
                () =>
                {
                    NetworkReadResult readResult;
                    if (syncReading)
                    {
                        readResult = this.reader.BeginReading();
                        Assert.That(readResult.SocketError, Is.EqualTo(SocketError.Interrupted));
                    }
                    else
                    {
                        this.reader.BeginReadingAsync();
                        Assert.That(this.readChannel.Receive(out readResult));
                        Assert.That(readResult.SocketError, Is.EqualTo(SocketError.OperationAborted));
                    }
                });
            Thread.Sleep(100);
            this.server.Dispose();
            this.reader.Dispose();
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
