// Copyright by Artur Wolf

using System.Diagnostics;
using System.Net;
using RpcNet;
using TestService;

const int CountProcesses = 100;
const int CountCalls = 10000;
const int Port = 2223;
IPAddress ipAddress = IPAddress.Loopback;

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
    return CallServer(ipAddress, protocol);
}

static void StartActualClients(string protocol)
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

static int CallServer(IPAddress ipAddress, string protocol)
{
    if (protocol.Equals("TCP", StringComparison.OrdinalIgnoreCase))
    {
        return CallTcpServer(ipAddress);
    }

    return CallUdpServer(ipAddress);
}

static int CallTcpServer(IPAddress ipAddress)
{
    int result = 0;
    using var testTcpClient = new TestServiceClient(Protocol.Tcp, ipAddress, Port);
    for (int i = 0; i < CountCalls; i++)
    {
        result += testTcpClient.Echo_1(i);
    }

    return result;
}

static int CallUdpServer(IPAddress ipAddress)
{
    int result = 0;
    using var testUdpClient = new TestServiceClient(Protocol.Udp, ipAddress, Port);
    for (int i = 0; i < CountCalls; i++)
    {
        result += testUdpClient.Echo_1(i);
    }

    return result;
}
