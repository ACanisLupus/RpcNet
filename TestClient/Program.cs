// Copyright by Artur Wolf

namespace TestClient;

using System.Net;
using RpcNet;
using Test;
using TestService;

internal class Program
{
    private static void Main()
    {
        var logger = new TestLogger("Test Client");
        using (var testTcpClient = new TestServiceClient(
                   Protocol.Tcp,
                   IPAddress.Loopback,
                   0,
                   new ClientSettings { Logger = logger }))
        {
            for (int i = 0; i < 2; i++)
            {
                testTcpClient.IntInt1_1(i);
            }
        }

        using var testUdpClient = new TestServiceClient(
            Protocol.Udp,
            IPAddress.Loopback,
            0,
            new ClientSettings { Logger = logger });
        for (int i = 0; i < 2; i++)
        {
            testUdpClient.IntInt1_1(i);
        }
    }
}
