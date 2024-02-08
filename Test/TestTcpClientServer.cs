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

        var clientSettings = new ClientSettings
        {
            //Logger = new TestLogger("TCP Client")
        };

        RpcException e = Assert.Throws<RpcException>(() => _ = new RpcTcpClient(_ipAddress, 1, Program, Version, clientSettings));

        Assert.That(e?.Message, Is.EqualTo("Could not connect to [::1]:1. Socket error code: ConnectionRefused."));
    }

    [Test]
    public void ServerShutdownWithoutException()
    {
        const int Program = 12;
        const int Version = 13;

        var serverSettings = new ServerSettings
        {
            //Logger = new TestLogger("TCP Server")
        };

        var server = new RpcTcpServer(
            _ipAddress,
            0,
            Program,
            new[]
            {
                Version
            },
            _ => { },
            serverSettings);
        Assert.DoesNotThrow(() => server.Dispose());
    }

    [Test]
    public void SendAndReceiveData()
    {
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
            //Logger = new TestLogger("TCP Server"),
            PortMapperPort = 0 // Don't register at port mapper
        };

        using var server = new RpcTcpServer(
            _ipAddress,
            0,
            Program,
            new[]
            {
                Version
            },
            Dispatcher,
            serverSettings);
        int port = server.Start();

        var clientSettings = new ClientSettings
        {
            //Logger = new TestLogger("TCP Client")
        };

        using var client = new RpcTcpClient(_ipAddress, port, Program, Version, clientSettings);
        var argument = new SimpleStruct
        {
            Value = 42
        };
        var result = new SimpleStruct();

        client.Call(Procedure, Version, argument, result);

        Assert.That(receivedCallChannel.TryReceive(TimeSpan.FromSeconds(10), out ReceivedRpcCall receivedCall));
        Assert.That(receivedCall.Procedure, Is.EqualTo(Procedure));
        Assert.That(receivedCall.Version, Is.EqualTo(Version));
        Assert.That(receivedCall.Caller, Is.Not.Null);

        Assert.That(argument.Value, Is.EqualTo(result.Value));
    }
}
