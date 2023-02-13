// Copyright by Artur Wolf

namespace Test;

using System.Net;
using NUnit.Framework;
using RpcNet;
using RpcNet.Internal;

[TestFixture]
internal class TestTcpClientServer
{
    private readonly IPAddress _ipAddress = IPAddress.Loopback;

    [Test]
    public void ServerIsNotRunning()
    {
        const int Program = 12;
        const int Version = 13;

        var clientSettings = new ClientSettings { Logger = new TestLogger("TCP Client") };

        RpcException exception = Assert.Throws<RpcException>(
            () => _ = new RpcTcpClient(_ipAddress, 1, Program, Version, clientSettings));

        Assert.That(
            exception.Message,
            Is.EqualTo($"Could not connect to {_ipAddress}:{1}. Socket error: ConnectionRefused."));
    }

    [Test]
    public void ServerShutdownWithoutException()
    {
        const int Program = 12;
        const int Version = 13;

        var serverSettings = new ServerSettings { Logger = new TestLogger("TCP Server") };

        var server = new RpcTcpServer(_ipAddress, 0, Program, new[] { Version }, call => { }, serverSettings);
        Assert.DoesNotThrow(() => server.Dispose());
    }
}
