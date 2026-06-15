// Copyright by Artur Wolf

namespace Test;

using System.Net;
using System.Threading.Channels;
using NUnit.Framework;
using RpcNet;
using RpcNet.Internal;
using TestService;

[TestFixture]
[TestFixtureSource(typeof(Utilities), nameof(Utilities.GetIpAddresses))]
internal sealed class TestTcpClientServer(IPAddress ipAddress)
{
    [Test]
    [CancelAfter(1)]
    public void ServerIsNotRunning()
    {
        CancellationToken ct = TestContext.CurrentContext.CancellationToken;

        const int Program = 12;
        const int Version = 13;

        Assert.CatchAsync<OperationCanceledException>(async () => await RpcTcpClient.ConnectAsync(ipAddress, 1, Program, Version, ClientSettings.Default, ct));
    }

    [Test]
    public void ServerShutdownWithoutException()
    {
        const int Program = 12;
        const int Version = 13;

        RpcTcpServer server = new(
            ipAddress,
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

        Channel<ReceivedRpcCall> receivedCallChannel = Channel.CreateUnbounded<ReceivedRpcCall>();

        ServerSettings serverSettings = new()
        {
            PortMapperPort = 0 // Don't register at port mapper
        };

        await using RpcTcpServer server = new(
            ipAddress,
            0,
            Program,
            [Version],
            Dispatcher,
            serverSettings);
        int port = await server.StartAsync(ct);

        using RpcTcpClient client = await RpcTcpClient.ConnectAsync(ipAddress, port, Program, Version, ClientSettings.Default, ct);
        SimpleStruct argument = new()
        {
            Value = 42
        };
        SimpleStruct result = new();

        await client.CallAsync(Procedure, Version, argument, result, ct);

        ReceivedRpcCall receivedCall = await receivedCallChannel.Reader.ReadAsync(ct);
        Assert.That(receivedCall.Procedure, Is.EqualTo(Procedure));
        Assert.That(receivedCall.Version, Is.EqualTo(Version));
        Assert.That(receivedCall.RpcEndPoint, Is.Not.Null);

        Assert.That(argument.Value, Is.EqualTo(result.Value));
        return;

        async ValueTask Dispatcher(ReceivedRpcCall call, CancellationToken cancellationToken)
        {
            // To assert it on the main thread
            await receivedCallChannel.Writer.WriteAsync(call, cancellationToken);

            SimpleStruct pingStruct = new();
            call.RetrieveCall(pingStruct);
            call.Reply(pingStruct);
        }
    }
}
