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
            const int program = 12;

            RpcException exception =
                Assert.Throws<RpcException>(() => _ = new RpcTcpClient(this.ipAddress, Port, program, TestLogger.Instance));

            Assert.That(
                exception.Message,
                Is.EqualTo($"Could not connect to {this.ipAddress}:{Port}. Socket error: ConnectionRefused."));
        }

        [Test]
        public void ServerShutdownWithoutException()
        {
            const int program = 12;
            const int version = 13;

            var server = new RpcTcpServer(this.ipAddress, Port, program, new[] { version }, call => { }, TestLogger.Instance);
            Assert.DoesNotThrow(() => server.Dispose());
        }

        [Test]
        public void TcpConnection()
        {
            const int program = 12;
            const int version = 13;
            const int procedure = 14;

            var receivedCallChannel = new Channel<ReceivedCall>();

            void Dispatcher(ReceivedCall call)
            {
                // To assert it on the main thread
                receivedCallChannel.Send(call);

                var pingStruct = new PingStruct();
                call.RetrieveCall(pingStruct);
                call.Reply(pingStruct);
            }

            using (new RpcTcpServer(this.ipAddress, Port, program, new[] { version }, Dispatcher, TestLogger.Instance))
            {
                for (int i = 0; i < 10; i++)
                {
                    using (var client = new RpcTcpClient(this.ipAddress, Port, program, TestLogger.Instance))
                    {
                        var argument = new PingStruct { Value = i };
                        var result = new PingStruct();

                        client.Call(procedure, version, argument, result);

                        Assert.That(receivedCallChannel.Receive(out ReceivedCall receivedCall));
                        Assert.That(receivedCall.Procedure, Is.EqualTo(procedure));
                        Assert.That(receivedCall.Version, Is.EqualTo(version));

                        Assert.That(argument.Value, Is.EqualTo(result.Value));
                    }
                }
            }
        }
    }
}
