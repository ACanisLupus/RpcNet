// Copyright by Artur Wolf

namespace Test;

using System.Net;
using NUnit.Framework;
using RpcNet;
using RpcNet.Internal;
using TestService;

[TestFixture]
internal sealed class TestTcpClientServer
{
    private readonly IPAddress _ipAddress = IPAddress.Loopback;

    [Test]
    public void ServerIsNotRunning()
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.Ignore("This test takes too long on Windows.");
        }

        const int Program = 12;
        const int Version = 13;

        RpcException? e = Assert.Throws<RpcException>(() => RpcTcpClient.Connect(_ipAddress, 1, Program, Version, ClientSettings.Default));

        Assert.That(e?.Message, Is.EqualTo("Could not connect to [::1]:1. Socket error code: ConnectionRefused."));
    }

    [Test]
    public void ServerShutdownWithoutException()
    {
        const int Program = 12;
        const int Version = 13;

        RpcTcpServer server = new(
            _ipAddress,
            0,
            Program,
            [Version],
            _ => { },
            ServerSettings.Default);
        Assert.DoesNotThrow(server.Dispose);
    }

    [Test]
    public void SendAndReceiveData()
    {
        const int Program = 12;
        const int Version = 13;
        const int Procedure = 14;

        Channel<ReceivedRpcCall> receivedCallChannel = new();

        ServerSettings serverSettings = new()
        {
            PortMapperPort = 0 // Don't register at port mapper
        };

        using RpcTcpServer server = new(
            _ipAddress,
            0,
            Program,
            [Version],
            Dispatcher,
            serverSettings);
        int port = server.Start();

        using RpcTcpClient client = RpcTcpClient.Connect(_ipAddress, port, Program, Version, ClientSettings.Default);
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
