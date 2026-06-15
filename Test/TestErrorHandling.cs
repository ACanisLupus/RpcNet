// Copyright by Artur Wolf

namespace Test;

using System.Net;
using NUnit.Framework;
using RpcNet;
using TestService;

[TestFixture]
[TestFixtureSource(typeof(Utilities), nameof(Utilities.GetProtocolAndAddressCases))]
internal sealed class TestErrorHandling(Protocol protocol, IPAddress ipAddress)
{
    [Test]
    public async ValueTask NonExistingProcedure()
    {
        CancellationToken ct = TestContext.CurrentContext.CancellationToken;
        await using TestServer testServer = await StartServerAsync(ct);

        int port = protocol == Protocol.Tcp ? testServer.TcpPort : testServer.UdpPort;

        using TestService2Client client = await TestService2Client.ConnectAsync(protocol, ipAddress, port, cancellationToken: ct);

        byte[] value = new byte[100];

        // ReSharper disable once AccessToDisposedClosure
        RpcException? e = Assert.ThrowsAsync<RpcException>(async () => await client.NonExistingProcedure_1Async(value, ct));
        Assert.That(e?.Message, Is.EqualTo("Call was unsuccessful: ProcedureUnavailable."));

        // Make sure the communication works after an error
        Assert.That(await client.Echo_1Async(42, ct), Is.EqualTo(42));
    }

    [Test]
    public async ValueTask NonExistingVersion()
    {
        CancellationToken ct = TestContext.CurrentContext.CancellationToken;
        await using TestServer testServer = await StartServerAsync(ct);

        int port = protocol == Protocol.Tcp ? testServer.TcpPort : testServer.UdpPort;

        using TestService2Client client = await TestService2Client.ConnectAsync(protocol, ipAddress, port, cancellationToken: ct);

        byte[] value = new byte[100];

        // ReSharper disable once AccessToDisposedClosure
        RpcException? e = Assert.ThrowsAsync<RpcException>(async () => await client.NonExistingProcedure_3Async(value, ct));
        Assert.That(e?.Message, Is.EqualTo("Call was unsuccessful: ProgramMismatch."));

        // Make sure the communication works after an error
        Assert.That(await client.Echo_1Async(42, ct), Is.EqualTo(42));
    }

    [Test]
    public async ValueTask ServerThrowsException()
    {
        CancellationToken ct = TestContext.CurrentContext.CancellationToken;
        await using TestServer testServer = await StartServerAsync(ct);

        int port = protocol == Protocol.Tcp ? testServer.TcpPort : testServer.UdpPort;

        using TestService2Client client = await TestService2Client.ConnectAsync(protocol, ipAddress, port, cancellationToken: ct);

        RpcException? e = Assert.ThrowsAsync<RpcException>(async () => await client.ThrowsException_1Async(ct));
        Assert.That(e?.Message, Is.EqualTo("Call was unsuccessful: SystemError."));

        // Make sure the communication works after an error
        Assert.That(await client.Echo_1Async(42, ct), Is.EqualTo(42));
    }

    private async ValueTask<TestServer> StartServerAsync(CancellationToken cancellationToken)
    {
        ServerSettings serverSettings = new()
        {
            PortMapperPort = 0
        };

        TestServer testServer = new(protocol, ipAddress, 0, serverSettings);
        await testServer.StartAsync(cancellationToken);
        return testServer;
    }
}
