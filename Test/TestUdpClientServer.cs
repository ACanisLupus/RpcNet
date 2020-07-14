namespace RpcNet.Test
{
    using System.Net;
    using NUnit.Framework;
    using RpcNet.Internal;
    using TestService;

    internal class TestUdpClientServer
    {
        [Test]
        public void SendAndReceiveData()
        {
            IPAddress ipAddress = IPAddress.Loopback;
            const int Port = 12345;
            const int Program = 12;
            const int Version = 13;
            const int Procedure = 14;

            var receivedCallChannel = new Channel<ReceivedCall>();

            void Dispatcher(ReceivedCall call)
            {
                // To assert it on the main thread
                receivedCallChannel.Send(call);

                var pingStruct = new PingStruct();
                call.RetrieveCall(pingStruct);
                call.Reply(pingStruct);
            }

            using (var server = new RpcUdpServer(
                ipAddress,
                Port,
                Program,
                new[] { Version },
                Dispatcher,
                TestLogger.Instance))
            {
                server.Start();
                using (var client = new RpcUdpClient(ipAddress, Port, Program, Version, TestLogger.Instance))
                {
                    var argument = new PingStruct { Value = 42 };
                    var result = new PingStruct();

                    client.Call(Procedure, Version, argument, result);

                    Assert.That(receivedCallChannel.Receive(out ReceivedCall receivedCall));
                    Assert.That(receivedCall.Procedure, Is.EqualTo(Procedure));
                    Assert.That(receivedCall.Version, Is.EqualTo(Version));
                    Assert.That(receivedCall.RemoteIpEndPoint, Is.Not.Null);

                    Assert.That(argument.Value, Is.EqualTo(result.Value));
                }
            }
        }
    }
}
