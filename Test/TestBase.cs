// Copyright by Artur Wolf

namespace Test;

using System.Net;
using NUnit.Framework;
using RpcNet;
using RpcNet.PortMapper;

internal class TestBase
{
    private PortMapperServer _portMapperServer;

    protected int PortMapperPort => _portMapperServer.TcpPort;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var settings = new ServerSettings { Logger = new TestLogger("Port Mapper") };

        _portMapperServer = new PortMapperServer(Protocol.Tcp, IPAddress.Loopback, 0, settings);
        _portMapperServer.Start();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown() => _portMapperServer?.Dispose();
}
