// Copyright by Artur Wolf

namespace Test;

using System.Net;
using NUnit.Framework;
using RpcNet;
using RpcNet.PortMapper;

[TestFixture]
[TestFixtureSource(typeof(Utilities), nameof(Utilities.GetProtocolAndAddressCases))]
internal sealed class TestPortMapper(Protocol protocol, IPAddress ipAddress)
{
    private int _portMapperPort;
    private PortMapperServer? _server;

    [SetUp]
    public async ValueTask SetUpAsync()
    {
        CancellationToken ct = TestContext.CurrentContext.CancellationToken;

        _portMapperPort = 0;
        _server = new PortMapperServer(protocol, ipAddress, _portMapperPort);
        await _server.StartAsync(ct);
        _portMapperPort = Utilities.GetPort(_server, protocol);
    }

    [TearDown]
    public async ValueTask TearDownAsync()
    {
        if (_server != null)
        {
            await _server.DisposeAsync();
        }
    }

    [Test]
    [TestCase(4711, 4712, ProtocolKind.Tcp, 4713)]
    [TestCase(4714, 4715, ProtocolKind.Udp, 4716)]
    [TestCase(4717, 4718, ProtocolKind.Tcp, 4719)]
    [TestCase(4720, 4721, ProtocolKind.Udp, 4721)]
    public async ValueTask TestSetAndGet(int port, int program, ProtocolKind protocolKind, int version)
    {
        CancellationToken ct = TestContext.CurrentContext.CancellationToken;

        using PortMapperClient client = await PortMapperClient.ConnectAsync(protocol, ipAddress, _portMapperPort, cancellationToken: ct);
        _ = await client.Set_2Async(
            new Mapping2
            {
                Port = port,
                ProgramNumber = program,
                Protocol = protocolKind,
                VersionNumber = version
            },
            ct);

        int receivedPort = await client.GetPort_2Async(
            new Mapping2
            {
                Protocol = protocolKind,
                ProgramNumber = program,
                VersionNumber = version
            },
            ct);

        Assert.That(receivedPort, Is.EqualTo(port));
    }

    [Test]
    [TestCase(1, 2, 3, 2, 42)]
    [TestCase(1, 2, 3, 42, 3)]
    public async ValueTask TestSetAndWrongGet(int port, int program, int version, int program2, int version2)
    {
        CancellationToken ct = TestContext.CurrentContext.CancellationToken;

        using PortMapperClient client = await PortMapperClient.ConnectAsync(protocol, ipAddress, _portMapperPort, cancellationToken: ct);
        _ = await client.Set_2Async(
            new Mapping2
            {
                Port = port,
                ProgramNumber = program,
                VersionNumber = version
            },
            ct);

        int receivedPort = await client.GetPort_2Async(
            new Mapping2
            {
                ProgramNumber = program2,
                VersionNumber = version2
            },
            ct);

        Assert.That(receivedPort, Is.EqualTo(0));
    }
}
