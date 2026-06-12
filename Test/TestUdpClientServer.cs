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

        Channel<ReceivedRpcCall> receivedCallChannel = new();

        ServerSettings serverSettings = new()
        {
            PortMapperPort = 0 // Don't register at port mapper
        };

        using RpcUdpServer server = new(
            ipAddress,
            0,
            Program,
            [Version],
            Dispatcher,
            serverSettings);
        int port = server.Start();

        using RpcUdpClient client = RpcUdpClient.Connect(ipAddress, port, Program, Version, ClientSettings.Default);
        SimpleStruct argument = new()
        {
            Value = 42
        };
        SimpleStruct result = new();

        client.Call(Procedure, Version, argument, result);

        Assert.That(receivedCallChannel.TryReceive(TimeSpan.FromSeconds(10), out ReceivedRpcCall? receivedCall));
        Assert.That(receivedCall, Is.Not.Null);
        Assert.That(receivedCall!.Procedure, Is.EqualTo(Procedure));
        Assert.That(receivedCall.Version, Is.EqualTo(Version));
        Assert.That(receivedCall.RpcEndPoint, Is.Not.Null);

        Assert.That(argument.Value, Is.EqualTo(result.Value));
        return;

        void Dispatcher(ReceivedRpcCall call)
        {
            // To assert it on the main thread
            receivedCallChannel.Send(call);

            SimpleStruct pingStruct = new();
            call.RetrieveCall(pingStruct);
            call.Reply(pingStruct);
        }
    }
}
