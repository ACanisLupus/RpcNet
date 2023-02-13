// Copyright by Artur Wolf

namespace PerformanceTestClient;

using System.Diagnostics;
using System.Net;
using RpcNet;
using TestService;

internal class Program
{
    private const int CountProcesses = 100;
    private const int CountCalls = 100000;
    private const int Port = 2223;

    private static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine($"usage: {Environment.ProcessPath} <TCP|UDP>");
            return 1;
        }

        if (args.Length == 1)
        {
            string protocol = args[0];
            StartActualClients(protocol);
            return 0;
        }
        else
        {
            string protocol = args[0];
            return CallServer(protocol);
        }
    }

    private static void StartActualClients(string protocol)
    {
        var stopwatch = Stopwatch.StartNew();

        List<Process> processes = new(CountProcesses);
        for (int i = 0; i < CountProcesses; i++)
        {
            Process process = new()
            {
                StartInfo =
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    FileName = Environment.ProcessPath!,
                    Arguments = protocol + " " + i
                }
            };
            process.Start();
            processes.Add(process);
        }

        for (int i = 0; i < CountProcesses; i++)
        {
            processes[i].WaitForExit();
        }

        stopwatch.Stop();
        Console.WriteLine($"Running {CountProcesses} clients calling {CountCalls} times took {stopwatch.Elapsed}.");
    }

    private static int CallServer(string protocol)
    {
        if (protocol.Equals("TCP", StringComparison.OrdinalIgnoreCase))
        {
            return CallTcpServer();
        }

        return CallUdpServer();
    }

    private static int CallTcpServer()
    {
        int result = 0;
        using var testTcpClient = new TestServiceClient(Protocol.Tcp, IPAddress.Loopback, Port);
        for (int i = 0; i < CountCalls; i++)
        {
            result += testTcpClient.IntInt1_1(i);
        }

        return result;
    }

    private static int CallUdpServer()
    {
        int result = 0;
        using var testUdpClient = new TestServiceClient(Protocol.Udp, IPAddress.Loopback, Port);
        for (int i = 0; i < CountCalls; i++)
        {
            result += testUdpClient.IntInt1_1(i);
        }

        return result;
    }
}
