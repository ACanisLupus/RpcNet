namespace TestClient
{
    using System;
    using System.Net;
    using RpcNet;
    using TestService;

    class Program
    {
        static void Main(string[] args)
        {
            using (var testClient = new TestServiceClient(Protocol.Tcp, IPAddress.Loopback))
            {
                for (int i = 0; i < 100; i++)
                {
                    var arg = new PingStruct
                    {
                        Value = i
                    };
                    var result = testClient.Ping_1(arg);
                    Console.WriteLine($"Sent: {i}, Received: {result.Value}");
                }
            }

            using (var testClient = new TestServiceClient(Protocol.Udp, IPAddress.Loopback))
            {
                for (int i = 0; i < 100; i++)
                {
                    var arg = new PingStruct
                    {
                        Value = i
                    };
                    var result = testClient.Ping_1(arg);
                    Console.WriteLine($"Sent: {i}, Received: {result.Value}");
                }
            }
        }
    }
}
