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
        CancellationToken ct = TestContext.CurrentContext.CancellationToken;

        if (OperatingSystem.IsWindows())
        {
            Assert.Ignore("This test takes too long on Windows.");
        }

        const int Program = 12;
        const int Version = 13;

        RpcException? e =
            Assert.ThrowsAsync<RpcException>(async () => await RpcTcpClient.ConnectAsync(_ipAddress, 1, Program, Version, ClientSettings.Default, ct));

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
            (_, _) => ValueTask.CompletedTask,
            ServerSettings.Default);
        Assert.DoesNotThrowAsync(async () => await server.DisposeAsync());
    }

    [Test]
    public async ValueTask SendAndReceiveData()
    {
        CancellationToken ct = TestContext.CurrentContext.CancellationToken;

        const int Program = 12;
        const int Version = 13;
        const int Procedure = 14;

        Channel<ReceivedRpcCall> receivedCallChannel = new();

        ServerSettings serverSettings = new()
        {
            PortMapperPort = 0 // Don't register at port mapper
        };

        await using RpcTcpServer server = new(
            _ipAddress,
            0,
            Program,
            [Version],
            Dispatcher,
            serverSettings);
        int port = await server.StartAsync(ct);

        using RpcTcpClient client = await RpcTcpClient.ConnectAsync(_ipAddress, port, Program, Version, ClientSettings.Default, ct);
        SimpleStruct argument = new()
        {
            Value = 42
        };
        SimpleStruct result = new();

        await client.CallAsync(Procedure, Version, argument, result, ct);

        Assert.That(receivedCallChannel.TryReceive(TimeSpan.FromSeconds(10), out ReceivedRpcCall? receivedCall));
        Assert.That(receivedCall, Is.Not.Null);
        Assert.That(receivedCall!.Procedure, Is.EqualTo(Procedure));
        Assert.That(receivedCall.Version, Is.EqualTo(Version));
        Assert.That(receivedCall.RpcEndPoint, Is.Not.Null);

        Assert.That(argument.Value, Is.EqualTo(result.Value));
        return;

        ValueTask Dispatcher(ReceivedRpcCall call, CancellationToken cancellationToken)
        {
            // To assert it on the main thread
            receivedCallChannel.Send(call);

            SimpleStruct pingStruct = new();
            call.RetrieveCall(pingStruct);
            call.Reply(pingStruct);
            return ValueTask.CompletedTask;
        }
    }
}
