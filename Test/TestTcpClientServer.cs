namespace RpcNet.Test
{
    using System;
    using System.Net;
    using NUnit.Framework;
    using RpcNet.Internal;
    using TestService;

    [TestFixture]
    internal class TestTcpClientServer
    {
        private const int Port = 12345;

        private readonly IPAddress ipAddress = IPAddress.Loopback;

        [Test]
        public void ServerIsNotRunning()
        {
            const int Program = 12;
            const int Version = 13;

            var clientSettings = new ClientSettings
            {
                Port = Port,
                Logger = new TestLogger("TCP Client")
            };

            var exception = Assert.Throws<RpcException>(
                () => _ = new RpcTcpClient(this.ipAddress, Program, Version, clientSettings));

            Assert.That(
                exception.Message,
                Is.EqualTo($"Could not connect to {this.ipAddress}:{Port}. Socket error: ConnectionRefused."));
        }

        [Test]
        public void ServerShutdownWithoutException()
        {
            const int Program = 12;
            const int Version = 13;

            var serverSettings = new ServerSettings
            {
                Logger = new TestLogger("TCP Server"),
                Port = Port
            };

            var server = new RpcTcpServer(this.ipAddress, Program, new[] { Version }, call => { }, serverSettings);
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

            var serverSettings = new ServerSettings
            {
                Logger = new TestLogger("TCP Server"),
                Port = Port
            };

            using var server = new RpcTcpServer(this.ipAddress, Program, new[] { Version }, Dispatcher, serverSettings);
            server.Start();

            for (int i = 0; i < 10; i++)
            {
                var clientSettings = new ClientSettings
                {
                    Port = Port,
                    Logger = new TestLogger("TCP Client")
                };

                using var client = new RpcTcpClient(this.ipAddress, Program, Version, clientSettings);
                var argument = new PingStruct { Value = i };
                var result = new PingStruct();

                client.Call(Procedure, Version, argument, result);

                Assert.That(receivedCallChannel.TryReceive(TimeSpan.FromSeconds(10), out ReceivedRpcCall receivedCall));
                Assert.That(receivedCall.Procedure, Is.EqualTo(Procedure));
                Assert.That(receivedCall.Version, Is.EqualTo(Version));

                Assert.That(argument.Value, Is.EqualTo(result.Value));
            }
        }
    }
}
