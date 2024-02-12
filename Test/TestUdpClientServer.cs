// Copyright by Artur Wolf

namespace Test;

using System.Net;
using NUnit.Framework;
using RpcNet;
using RpcNet.Internal;
using TestService;

[TestFixture]
internal sealed class TestUdpClientServer
{
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

            var pingStruct = new SimpleStruct();
            call.RetrieveCall(pingStruct);
            call.Reply(pingStruct);
        }

        var serverSettings = new ServerSettings
        {
            PortMapperPort = 0 // Don't register at port mapper
        };

        using var server = new RpcUdpServer(
            ipAddress,
            0,
            Program,
            new[]
            {
                Version
            },
            Dispatcher,
            serverSettings);
        int port = server.Start();

        using var client = new RpcUdpClient(ipAddress, port, Program, Version);
        var argument = new SimpleStruct
        {
            Value = 42
        };
        var result = new SimpleStruct();

        client.Call(Procedure, Version, argument, result);

        Assert.That(receivedCallChannel.TryReceive(TimeSpan.FromSeconds(10), out ReceivedRpcCall receivedCall));
        Assert.That(receivedCall.Procedure, Is.EqualTo(Procedure));
        Assert.That(receivedCall.Version, Is.EqualTo(Version));
        Assert.That(receivedCall.RpcEndPoint, Is.Not.Null);

        Assert.That(argument.Value, Is.EqualTo(result.Value));
    }
}
