// Copyright by Artur Wolf

using System.Diagnostics;
using System.Net;
using RpcNet;
using TestService;

const int CountProcesses = 100;
const int CountCalls = 10000;
const int Port = 2223;
IPAddress ipAddress = IPAddress.Loopback;

switch (args.Length)
{
    case 0:
        Console.WriteLine($"usage: {Environment.ProcessPath} <TCP|UDP>");
        return 1;
    case 1:
        {
            string protocol = args[0];
            StartActualClients(protocol);
            return 0;
        }
    default:
        {
            string protocol = args[0];
            return await CallServer(ipAddress, protocol).ConfigureAwait(false);
        }
}

static void StartActualClients(string protocol)
{
    Stopwatch stopwatch = Stopwatch.StartNew();

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
        _ = process.Start();
        processes.Add(process);
    }

    for (int i = 0; i < CountProcesses; i++)
    {
        processes[i].WaitForExit();
    }

    stopwatch.Stop();
    Console.WriteLine($"Running {CountProcesses} clients calling {CountCalls} times took {stopwatch.Elapsed}.");
}

static async ValueTask<int> CallServer(IPAddress ipAddress, string protocol)
{
    if (protocol.Equals("TCP", StringComparison.OrdinalIgnoreCase))
    {
        return await CallTcpServer(ipAddress).ConfigureAwait(false);
    }

    return await CallUdpServer(ipAddress).ConfigureAwait(false);
}

static async ValueTask<int> CallTcpServer(IPAddress ipAddress)
{
    int result = 0;
    using TestServiceClient testTcpClient = await TestServiceClient.ConnectAsync(Protocol.Tcp, ipAddress, Port).ConfigureAwait(false);
    for (int i = 0; i < CountCalls; i++)
    {
        result += await testTcpClient.Echo_1Async(i).ConfigureAwait(false);
    }

    return result;
}

static async ValueTask<int> CallUdpServer(IPAddress ipAddress)
{
    int result = 0;
    using TestServiceClient testUdpClient = await TestServiceClient.ConnectAsync(Protocol.Udp, ipAddress, Port).ConfigureAwait(false);
    for (int i = 0; i < CountCalls; i++)
    {
        result += await testUdpClient.Echo_1Async(i).ConfigureAwait(false);
    }

    return result;
}
