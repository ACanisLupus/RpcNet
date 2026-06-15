// Copyright by Artur Wolf

namespace Test;

using System.Net;
using NUnit.Framework;
using RpcNet;
using TestService;

internal sealed class TestErrorHandling
{
    private readonly IPAddress _ipAddress = IPAddress.Loopback;

    private TestServer? _testServer;

    private TestServer TestServer => _testServer ?? throw new InvalidOperationException("Test server is not initialized.");

    [SetUp]
    public async ValueTask SetUpAsync()
    {
        CancellationToken ct = TestContext.CurrentContext.CancellationToken;

        ServerSettings serverSettings = new()
        {
            PortMapperPort = 0
        };

        _testServer = new TestServer(Protocol.Tcp | Protocol.Udp, _ipAddress, 0, serverSettings);
        await _testServer.StartAsync(ct);
    }

    [TearDown]
    public async ValueTask TearDownAsync()
    {
        if (_testServer != null)
        {
            await _testServer.DisposeAsync();
        }
    }

    [Test]
    [TestCase(Protocol.Tcp)]
    [TestCase(Protocol.Udp)]
    public async ValueTask NonExistingProcedure(Protocol protocol)
    {
        CancellationToken ct = TestContext.CurrentContext.CancellationToken;

        int port = protocol == Protocol.Tcp ? TestServer.TcpPort : TestServer.UdpPort;

        using TestService2Client client = await TestService2Client.ConnectAsync(protocol, _ipAddress, port, cancellationToken: ct);

        // For TCP, the argument is bigger than the internal buffer size, so that the handling of buffer overflows is checked as well
        byte[] value = protocol == Protocol.Tcp ? new byte[128000] : new byte[100];

        // ReSharper disable once AccessToDisposedClosure
        RpcException? e = Assert.ThrowsAsync<RpcException>(async () => await client.NonExistingProcedure_1Async(value, ct));
        Assert.That(e?.Message, Is.EqualTo("Call was unsuccessful: ProcedureUnavailable."));

        // Make sure the communication works after an error
        Assert.That(await client.Echo_1Async(42, ct), Is.EqualTo(42));
    }

    [Test]
    [TestCase(Protocol.Tcp)]
    [TestCase(Protocol.Udp)]
    public async ValueTask NonExistingVersion(Protocol protocol)
    {
        CancellationToken ct = TestContext.CurrentContext.CancellationToken;

        int port = protocol == Protocol.Tcp ? TestServer.TcpPort : TestServer.UdpPort;

        using TestService2Client client = await TestService2Client.ConnectAsync(protocol, _ipAddress, port, cancellationToken: ct);

        // For TCP, the argument is bigger than the internal buffer size, so that the handling of buffer overflows is checked as well
        byte[] value = protocol == Protocol.Tcp ? new byte[128000] : new byte[100];

        // ReSharper disable once AccessToDisposedClosure
        RpcException? e = Assert.ThrowsAsync<RpcException>(async () => await client.NonExistingProcedure_3Async(value, ct));
        Assert.That(e?.Message, Is.EqualTo("Call was unsuccessful: ProgramMismatch."));

        // Make sure the communication works after an error
        Assert.That(await client.Echo_1Async(42, ct), Is.EqualTo(42));
    }

    [Test]
    [TestCase(Protocol.Tcp)]
    [TestCase(Protocol.Udp)]
    public async ValueTask ServerThrowsException(Protocol protocol)
    {
        CancellationToken ct = TestContext.CurrentContext.CancellationToken;

        int port = protocol == Protocol.Tcp ? TestServer.TcpPort : TestServer.UdpPort;

        using TestService2Client client = await TestService2Client.ConnectAsync(protocol, _ipAddress, port, cancellationToken: ct);

        RpcException? e = Assert.ThrowsAsync<RpcException>(async () => await client.ThrowsException_1Async(ct));
        Assert.That(e?.Message, Is.EqualTo("Call was unsuccessful: SystemError."));

        // Make sure the communication works after an error
        Assert.That(await client.Echo_1Async(42, ct), Is.EqualTo(42));
    }
}
