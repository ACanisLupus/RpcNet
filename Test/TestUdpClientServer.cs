namespace Test
{
    using System;
    using System.Net;
    using NUnit.Framework;
    using RpcNet;
    using RpcNet.Internal;
    using TestService;

    [TestFixture]
    internal class TestUdpClientServer
    {
        private const int Port = 12345;

        [Test]
        public void SendAndReceiveData()
        {
            IPAddress ipAddress = IPAddress.Loopback;
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
                Logger = new TestLogger("UDP Server"),
                Port = Port
            };

            using var server = new RpcUdpServer(ipAddress, Program, new[] { Version }, Dispatcher, serverSettings);
            server.Start();

            var clientSettings = new ClientSettings
            {
                Port = Port,
                Logger = new TestLogger("UDP Client")
            };

            using var client = new RpcUdpClient(ipAddress, Program, Version, clientSettings);
            var argument = new PingStruct { Value = 42 };
            var result = new PingStruct();

            client.Call(Procedure, Version, argument, result);

            Assert.That(receivedCallChannel.TryReceive(TimeSpan.FromSeconds(10), out ReceivedRpcCall receivedCall));
            Assert.That(receivedCall.Procedure, Is.EqualTo(Procedure));
            Assert.That(receivedCall.Version, Is.EqualTo(Version));
            Assert.That(receivedCall.Caller, Is.Not.Null);

            Assert.That(argument.Value, Is.EqualTo(result.Value));
        }
    }
}
