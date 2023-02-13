// Copyright by Artur Wolf

namespace PortMapperTestServer;

using System.Net;
using System.Threading;
using RpcNet;
using RpcNet.PortMapper;
using Test;

internal class Program
{
    private static void Main(string[] args)
    {
        if ((args.Length != 1) || !IPEndPoint.TryParse(args[0], out IPEndPoint ipEndPoint))
        {
            ipEndPoint = new IPEndPoint(IPAddress.Any, 111);
        }

        using var portMapperServer = new PortMapperServer(
            Protocol.TcpAndUdp,
            ipEndPoint.Address,
            ipEndPoint.Port,
            new ServerSettings { Logger = new TestLogger("Port Mapper") });
        portMapperServer.Start();

        Thread.Sleep(-1);
    }
}
