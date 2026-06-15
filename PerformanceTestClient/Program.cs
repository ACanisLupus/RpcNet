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
            if (!Enum.TryParse(args[0], true, out Protocol protocol))
            {
                Console.WriteLine($"Invalid protocol: {args[0]}");
                return 1;
            }

            StartActualClients(protocol);
            return 0;
        }
    default:
        {
            if (!Enum.TryParse(args[0], true, out Protocol protocol))
            {
                Console.WriteLine($"Invalid protocol: {args[0]}");
                return 1;
            }

            await CallServer(ipAddress, protocol).ConfigureAwait(false);
            return 0;
        }
}

static void StartActualClients(Protocol protocol)
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
                Arguments = $"{protocol} {i}"
            }
        };
        _ = process.Start();
        processes.Add(process);
    }

    for (int i = 0; i < CountProcesses; i++)
    {
        processes[i].WaitForExit();
        if (processes[i].ExitCode != 0)
        {
            Console.WriteLine($"Process {i} exited with code {processes[i].ExitCode}");
        }
    }

    stopwatch.Stop();
    Console.WriteLine($"Running {CountProcesses} clients calling {CountCalls} times took {stopwatch.Elapsed}.");
}

static async ValueTask CallServer(IPAddress ipAddress, Protocol protocol)
{
    using TestServiceClient testTcpClient = await TestServiceClient.ConnectAsync(protocol, ipAddress, Port).ConfigureAwait(false);

    for (int i = 0; i < CountCalls; i++)
    {
        int result = await testTcpClient.Echo_1Async(i).ConfigureAwait(false);
        if (result != i)
        {
            throw new InvalidOperationException($"Unexpected result: {result}");
        }
    }
}
