namespace TestClient
{
    using System;
    using System.Net;
    using RpcNet;
    using TestService;

    internal class Program
    {
        private static void Main()
        {
            using (var testTcpClient = new TestServiceClient(Protocol.Tcp, IPAddress.Loopback))
            {
                for (int i = 0; i < 10; i++)
                {
                    var arg = new PingStruct
                    {
                        Value = i
                    };
                    PingStruct result = testTcpClient.Ping_1(arg);
                    Console.WriteLine($"PING1 TCP - Sent: {i}, Received: {result.Value}");

                    result = testTcpClient.Ping2_2(arg);
                    Console.WriteLine($"PING2 TCP - Sent: {i}, Received: {result.Value}");
                }
            }

            using var testUdpClient = new TestServiceClient(Protocol.Udp, IPAddress.Loopback);
            for (int i = 0; i < 10; i++)
            {
                var arg = new PingStruct
                {
                    Value = i
                };
                PingStruct result = testUdpClient.Ping_1(arg);
                Console.WriteLine($"PING1 UDP - Sent: {i}, Received: {result.Value}");

                result = testUdpClient.Ping2_2(arg);
                Console.WriteLine($"PING2 UDP - Sent: {i}, Received: {result.Value}");
            }
        }
    }
}
