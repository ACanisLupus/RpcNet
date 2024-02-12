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

        RpcException e = Assert.Throws<RpcException>(() => _ = new RpcTcpClient(_ipAddress, 1, Program, Version));

        Assert.That(e?.Message, Is.EqualTo("Could not connect to [::1]:1. Socket error code: ConnectionRefused."));
    }

    [Test]
    public void ServerShutdownWithoutException()
    {
        const int Program = 12;
        const int Version = 13;

        var server = new RpcTcpServer(
            _ipAddress,
            0,
            Program,
            new[]
            {
                Version
            },
            _ => { });
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

        using var client = new RpcTcpClient(_ipAddress, port, Program, Version);
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
