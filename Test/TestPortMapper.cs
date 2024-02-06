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
    private PortMapperServer _server;

    [SetUp]
    public void SetUp()
    {
        _portMapperPort = 0;
        var settings = new ServerSettings { Logger = new TestLogger("Port Mapper") };
        _server = new PortMapperServer(Protocol.TcpAndUdp, _ipAddress, _portMapperPort, settings);
        _server.Start();
        _portMapperPort = _server.TcpPort;
    }

    [TearDown]
    public void TearDown() => Interlocked.Exchange(ref _server, null)?.Dispose();

    [Test]
    [TestCase(4711, 4712, ProtocolKind.Tcp, 4713)]
    [TestCase(4714, 4715, ProtocolKind.Udp, 4716)]
    [TestCase(4717, 4718, ProtocolKind.Tcp, 4719)]
    [TestCase(4720, 4721, ProtocolKind.Udp, 4721)]
    public void TestSetAndGet(int port, int program, ProtocolKind protocol, int version)
    {
        using var client = new PortMapperClient(Protocol.Tcp, _ipAddress, _portMapperPort);
        client.Set_2(new Mapping2 { Port = port, ProgramNumber = program, Protocol = protocol, VersionNumber = version });

        int receivedPort = client.GetPort_2(new Mapping2 { Protocol = protocol, ProgramNumber = program, VersionNumber = version });

        Assert.That(receivedPort, Is.EqualTo(port));
    }

    [Test]
    [TestCase(1, 2, 3, 2, 42)]
    [TestCase(1, 2, 3, 42, 3)]
    public void TestSetAndWrongGet(int port, int program, int version, int program2, int version2)
    {
        using var client = new PortMapperClient(Protocol.Tcp, _ipAddress, _portMapperPort);
        client.Set_2(new Mapping2 { Port = port, ProgramNumber = program, VersionNumber = version });

        int receivedPort = client.GetPort_2(new Mapping2 { ProgramNumber = program2, VersionNumber = version2 });

        Assert.That(receivedPort, Is.EqualTo(0));
    }
}
