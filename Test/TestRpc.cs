// Copyright by Artur Wolf

namespace Test;

using System.Net;
using NUnit.Framework;
using RpcNet;
using RpcNet.PortMapper;
using TestService;

[TestFixture]
[TestFixtureSource(typeof(Utilities), nameof(Utilities.GetProtocolAndAddressCases))]
internal sealed class TestRpc(Protocol protocol, IPAddress ipAddress)
{
    private PortMapperServer? _portMapperServer;
    private TestServer? _testServer;

    private PortMapperServer PortMapperServer => _portMapperServer ?? throw new InvalidOperationException("Port mapper server is not initialized.");

    [SetUp]
    public async ValueTask SetUp()
    {
        CancellationToken ct = TestContext.CurrentContext.CancellationToken;

        ServerSettings portMapperSettings = new()
        {
            Logger = new TestLogger("Port Mapper")
        };

        _portMapperServer = new PortMapperServer(protocol, ipAddress, 0, portMapperSettings);
        await _portMapperServer.StartAsync(ct);

        ServerSettings serverSettings = new()
        {
            PortMapperPort = Utilities.GetPort(_portMapperServer, protocol)
        };

        _testServer = new TestServer(protocol, ipAddress, 0, serverSettings);
        await _testServer.StartAsync(ct);
    }

    [TearDown]
    public async ValueTask TearDownAsync()
    {
        if (_testServer != null)
        {
            await _testServer.DisposeAsync();
        }

        if (_portMapperServer != null)
        {
            await _portMapperServer.DisposeAsync();
        }
    }

    [Test]
    public async ValueTask OneClient()
    {
        CancellationToken ct = TestContext.CurrentContext.CancellationToken;

        ClientSettings clientSettings = new()
        {
            PortMapperPort = Utilities.GetPort(PortMapperServer, protocol)
        };

        using TestServiceClient client = await TestServiceClient.ConnectAsync(protocol, ipAddress, 0, clientSettings, ct);
        SimpleStruct result = await client.SimpleStructSimpleStruct_2Async(
            new SimpleStruct
            {
                Value = 42
            },
            ct);
        Assert.That(result.Value, Is.EqualTo(42));
    }
}
