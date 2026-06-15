// Copyright by Artur Wolf

namespace Test;

using System.Net;
using NUnit.Framework;
using RpcNet;
using RpcNet.PortMapper;

[TestFixture]
internal sealed class TestPortMapper
{
    private readonly IPAddress _ipAddress = IPAddress.Loopback;

    private int _portMapperPort;
    private PortMapperServer? _server;

    [SetUp]
    public async ValueTask SetUpAsync()
    {
        CancellationToken ct = TestContext.CurrentContext.CancellationToken;

        _portMapperPort = 0;
        _server = new PortMapperServer(Protocol.Tcp | Protocol.Udp, _ipAddress, _portMapperPort);
        await _server.StartAsync(ct);
        _portMapperPort = _server.TcpPort;
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
    public async ValueTask TestSetAndGet(int port, int program, ProtocolKind protocol, int version)
    {
        CancellationToken ct = TestContext.CurrentContext.CancellationToken;

        using PortMapperClient client = await PortMapperClient.ConnectAsync(Protocol.Tcp, _ipAddress, _portMapperPort, cancellationToken: ct);
        _ = await client.Set_2Async(
            new Mapping2
            {
                Port = port,
                ProgramNumber = program,
                Protocol = protocol,
                VersionNumber = version
            },
            ct);

        int receivedPort = await client.GetPort_2Async(
            new Mapping2
            {
                Protocol = protocol,
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

        using PortMapperClient client = await PortMapperClient.ConnectAsync(Protocol.Tcp, _ipAddress, _portMapperPort, cancellationToken: ct);
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
