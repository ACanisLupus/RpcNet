namespace RpcNet.Test
{
    using System.Net;
    using NUnit.Framework;
    using RpcNet.Internal;
    using TestService;

    internal class TestTcpClientServer
    {
        private const int Port = 12345;

        private readonly IPAddress ipAddress = IPAddress.Loopback;

        [Test]
        public void ServerIsNotRunning()
        {
            const int Program = 12;
            const int Version = 13;

            var exception = Assert.Throws<RpcException>(
                () => _ = new RpcTcpClient(this.ipAddress, Port, Program, Version, new TestLogger("TCP Server")));

            Assert.That(
                exception.Message,
                Is.EqualTo($"Could not connect to {this.ipAddress}:{Port}. Socket error: ConnectionRefused."));
        }

        [Test]
        public void ServerShutdownWithoutException()
        {
            const int Program = 12;
            const int Version = 13;

            var server = new RpcTcpServer(
                this.ipAddress,
                Port,
                Program,
                new[] { Version },
                call => { },
                new TestLogger("TCP Server"));
            Assert.DoesNotThrow(() => server.Dispose());
        }

        [Test]
        public void TcpConnection()
        {
            const int Program = 12;
            const int Version = 13;
            const int Procedure = 14;

            var receivedCallChannel = new Channel<ReceivedRpcCall>();

            void Dispatcher(ReceivedRpcCall call)
            {
                // To assert it on the main thread
                receivedCallChannel.Send(call);

                var pingStruct = new PingStruct();
                call.RetrieveCall(pingStruct);
                call.Reply(pingStruct);
            }

            using var server = new RpcTcpServer(
                this.ipAddress,
                Port,
                Program,
                new[] { Version },
                Dispatcher,
                new TestLogger("TCP Server"));
            server.Start();

            for (int i = 0; i < 10; i++)
            {
                using var client = new RpcTcpClient(
                    this.ipAddress,
                    Port,
                    Program,
                    Version,
                    new TestLogger("TCP Client"));
                var argument = new PingStruct { Value = i };
                var result = new PingStruct();

                client.Call(Procedure, Version, argument, result);

                Assert.That(receivedCallChannel.Receive(out ReceivedRpcCall receivedCall));
                Assert.That(receivedCall.Procedure, Is.EqualTo(Procedure));
                Assert.That(receivedCall.Version, Is.EqualTo(Version));

                Assert.That(argument.Value, Is.EqualTo(result.Value));
            }
        }
    }
}
