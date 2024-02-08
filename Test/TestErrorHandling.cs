// Copyright by Artur Wolf

namespace Test;

using NUnit.Framework;
using RpcNet;
using System.Net;
using TestService;

internal sealed class TestErrorHandling
{
    private readonly IPAddress _ipAddress = IPAddress.Loopback;

    private TestServer _testServer;

    [SetUp]
    public void SetUp()
    {
        var serverSettings = new ServerSettings
        {
            PortMapperPort = 0,
            //Logger = new TestLogger("Test Server")
        };

        _testServer = new TestServer(Protocol.TcpAndUdp, _ipAddress, 0, serverSettings);
        _testServer.Start();
    }

    [TearDown]
    public void TearDown() => _testServer?.Dispose();

    [Test]
    [TestCase(Protocol.Tcp)]
    [TestCase(Protocol.Udp)]
    public void NonExistingProcedure(Protocol protocol)
    {
        int port = protocol == Protocol.Tcp ? _testServer.TcpPort : _testServer.UdpPort;

        var clientSettings = new ClientSettings
        {
            //Logger = new TestLogger("Test Client")
        };

        using var client = new TestService2Client(protocol, _ipAddress, port, clientSettings);

        // For TCP, the argument is bigger than the internal buffer size, so that the handling of buffer overflows is checked as well
        byte[] value = protocol == Protocol.Tcp ? new byte[128000] : new byte[100];

        RpcException e = Assert.Throws<RpcException>(() => client.NonExistingProcedure_1(value));
        Assert.That(e?.Message, Is.EqualTo("Call was unsuccessful: ProcedureUnavailable."));

        // Make sure the communication works after an error
        Assert.That(client.Echo_1(42), Is.EqualTo(42));
    }

    [Test]
    [TestCase(Protocol.Tcp)]
    [TestCase(Protocol.Udp)]
    public void NonExistingVersion(Protocol protocol)
    {
        int port = protocol == Protocol.Tcp ? _testServer.TcpPort : _testServer.UdpPort;

        var clientSettings = new ClientSettings
        {
            //Logger = new TestLogger("Test Client")
        };

        using var client = new TestService2Client(protocol, _ipAddress, port, clientSettings);

        // For TCP, the argument is bigger than the internal buffer size, so that the handling of buffer overflows is checked as well
        byte[] value = protocol == Protocol.Tcp ? new byte[128000] : new byte[100];

        RpcException e = Assert.Throws<RpcException>(() => client.NonExistingProcedure_3(value));
        Assert.That(e?.Message, Is.EqualTo("Call was unsuccessful: ProgramMismatch."));

        // Make sure the communication works after an error
        Assert.That(client.Echo_1(42), Is.EqualTo(42));
    }

    [Test]
    [TestCase(Protocol.Tcp)]
    [TestCase(Protocol.Udp)]
    public void ServerThrowsException(Protocol protocol)
    {
        int port = protocol == Protocol.Tcp ? _testServer.TcpPort : _testServer.UdpPort;

        var clientSettings = new ClientSettings
        {
            //Logger = new TestLogger("Test Client")
        };

        using var client = new TestService2Client(protocol, _ipAddress, port, clientSettings);

        RpcException e = Assert.Throws<RpcException>(() => client.ThrowsException_1());
        Assert.That(e?.Message, Is.EqualTo("Call was unsuccessful: SystemError."));

        // Make sure the communication works after an error
        Assert.That(client.Echo_1(42), Is.EqualTo(42));
    }
}
