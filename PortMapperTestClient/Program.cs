// Copyright by Artur Wolf

using System.Diagnostics;
using System.Net;
using RpcNet;
using RpcNet.PortMapper;

CancellationToken ct = CancellationToken.None;

List<string> arguments = [.. args];
if (!TryReadCommand(arguments, out Command command) || !TryReadIpEndPoint(arguments, out IPEndPoint ipEndPoint))
{
    PrintUsage();
    return 1;
}

return command switch
{
    Command.Dump => await DumpAsync(ipEndPoint, ct).ConfigureAwait(false),
    Command.Get => await GetAsync(ipEndPoint, arguments, ct).ConfigureAwait(false),
    Command.Set => await SetAsync(ipEndPoint, arguments, ct).ConfigureAwait(false),
    Command.Unset => await UnsetAsync(ipEndPoint, arguments, ct).ConfigureAwait(false),
    _ => 0
};

static bool TryReadCommand(List<string> args, out Command command)
{
    command = Command.Unknown;

    if (args.Count == 0)
    {
        return false;
    }

    if (!Enum.TryParse(args[0], true, out command) || (command == Command.Unknown))
    {
        return false;
    }

    args.RemoveAt(0);
    return true;
}

static bool TryReadIpEndPoint(List<string> args, out IPEndPoint ipEndPoint)
{
    ipEndPoint = new IPEndPoint(IPAddress.Loopback, 111);

    if (args.Count == 0)
    {
        return true;
    }

    if (int.TryParse(args[0], out int _))
    {
        return true;
    }

    if (!IPEndPoint.TryParse(args[0], out IPEndPoint? differentIpEndPoint))
    {
        return false;
    }

    ipEndPoint = differentIpEndPoint;
    args.RemoveAt(0);
    return true;
}

static async ValueTask<int> DumpAsync(IPEndPoint ipEndPoint, CancellationToken cancellationToken)
{
    using PortMapperClient client = await CreateClientAsync(ipEndPoint, cancellationToken).ConfigureAwait(false);
    MappingNodeHead2 mappingNodeHead = await client.Dump_2Async(cancellationToken).ConfigureAwait(false);
    MappingNode2? node = mappingNodeHead.Value;
    while (node is not null)
    {
        Mapping2 mapping = node.Mapping;
        Console.WriteLine(mapping);
        node = node.Next;
    }

    return 0;
}

static async ValueTask<int> GetAsync(IPEndPoint ipEndPoint, List<string> args, CancellationToken cancellationToken)
{
    if ((args.Count != 3) ||
        !int.TryParse(args[0], out int program) ||
        !int.TryParse(args[1], out int version) ||
        !Enum.TryParse(args[2], true, out ProtocolKind protocol))
    {
        PrintUsage();
        return 1;
    }

    using PortMapperClient client = await CreateClientAsync(ipEndPoint, cancellationToken).ConfigureAwait(false);
    int result = await client.GetPort_2Async(
            new Mapping2
            {
                ProgramNumber = program,
                VersionNumber = version,
                Protocol = protocol
            },
            cancellationToken)
        .ConfigureAwait(false);

    Console.WriteLine(result);
    return 0;
}

static async ValueTask<int> SetAsync(IPEndPoint ipEndPoint, List<string> args, CancellationToken cancellationToken)
{
    if ((args.Count != 4) ||
        !int.TryParse(args[0], out int program) ||
        !int.TryParse(args[1], out int version) ||
        !Enum.TryParse(args[2], true, out ProtocolKind protocol) ||
        !int.TryParse(args[3], out int port))
    {
        PrintUsage();
        return 1;
    }

    using PortMapperClient client = await CreateClientAsync(ipEndPoint, cancellationToken).ConfigureAwait(false);
    bool result = await client.Set_2Async(
            new Mapping2
            {
                ProgramNumber = program,
                VersionNumber = version,
                Protocol = protocol,
                Port = port
            },
            cancellationToken)
        .ConfigureAwait(false);

    Console.WriteLine(result);
    return 0;
}

static async ValueTask<int> UnsetAsync(IPEndPoint ipEndPoint, List<string> args, CancellationToken cancellationToken)
{
    if (args.Count is < 2 or > 3 || !int.TryParse(args[0], out int program) || !int.TryParse(args[1], out int version))
    {
        PrintUsage();
        return 1;
    }

    ProtocolKind protocol = ProtocolKind.Unknown;
    if ((args.Count == 3) && !Enum.TryParse(args[2], true, out protocol))
    {
        PrintUsage();
        return 1;
    }

    using PortMapperClient client = await CreateClientAsync(ipEndPoint, cancellationToken).ConfigureAwait(false);
    bool result = await client.Unset_2Async(
            new Mapping2
            {
                ProgramNumber = program,
                VersionNumber = version,
                Protocol = protocol
            },
            cancellationToken)
        .ConfigureAwait(false);

    Console.WriteLine(result);
    return 0;
}

static ValueTask<PortMapperClient> CreateClientAsync(IPEndPoint ipEndPoint, CancellationToken cancellationToken)
{
    return PortMapperClient.ConnectAsync(Protocol.Tcp, ipEndPoint.Address, ipEndPoint.Port, cancellationToken: cancellationToken);
}

static void PrintUsage()
{
    string processName = Process.GetCurrentProcess().ProcessName;
    const string Ip = "[<ip address>[:port]]";
    Console.WriteLine("Usage:");
    Console.WriteLine($"  {processName} dump {Ip}");
    Console.WriteLine($"  {processName} get {Ip} <program> <version> <protocol>");
    Console.WriteLine($"  {processName} set {Ip} <program> <version> <protocol> <port>");
    Console.WriteLine($"  {processName} unset {Ip} <program> <version> [<port>]");
}

internal enum Command
{
    Unknown,
    Dump,
    Get,
    Set,
    Unset
}
