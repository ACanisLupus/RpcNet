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
            int port = 12345;
            int program = 12;
            int version = 13;
            int procedure = 14;

            var receivedCallChannel = new Channel<ReceivedCall>();

            void Dispatcher(ReceivedCall call)
            {
                // To assert it on the main thread
                receivedCallChannel.Send(call);

                var pingStruct = new PingStruct();
                call.RetrieveCall(pingStruct);
                call.Reply(pingStruct);
            }

            using (new RpcUdpServer(ipAddress, port, program, new[] { version }, Dispatcher, TestLogger.Instance))
            {
                using (var client = new RpcUdpClient(ipAddress, port, program, TestLogger.Instance))
                {
                    var argument = new PingStruct { Value = 42 };
                    var result = new PingStruct();

                    client.Call(procedure, version, argument, result);

                    Assert.That(receivedCallChannel.Receive(out ReceivedCall receivedCall));
                    Assert.That(receivedCall.Procedure, Is.EqualTo(procedure));
                    Assert.That(receivedCall.Version, Is.EqualTo(version));
                    Assert.That(receivedCall.RemoteIpEndPoint, Is.Not.Null);

                    Assert.That(argument.Value, Is.EqualTo(result.Value));
                }
            }
        }
    }
}
