// Copyright by Artur Wolf

namespace PortMapperTestClient;

using System.Diagnostics;
using System.Net;
using RpcNet;
using RpcNet.PortMapper;

internal class Program
{
    private static int Main(string[] args)
    {
        var arguments = new List<string>(args);
        if (!TryReadCommand(arguments, out Command command))
        {
            PrintUsage();
            return 1;
        }

        if (!TryReadIpEndPoint(arguments, out IPEndPoint ipEndPoint))
        {
            PrintUsage();
            return 1;
        }

        return command switch
        {
            Command.Dump => Dump(ipEndPoint),
            Command.Get => Get(ipEndPoint, arguments),
            Command.Set => Set(ipEndPoint, arguments),
            Command.Unset => Unset(ipEndPoint, arguments),
            _ => 0
        };
    }

    private static bool TryReadCommand(List<string> args, out Command command)
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

    private static bool TryReadIpEndPoint(IList<string> args, out IPEndPoint ipEndPoint)
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

    private static int Dump(IPEndPoint ipEndPoint)
    {
        using PortMapperClient client = CreateClient(ipEndPoint);
        MappingNodeHead2 mappingNodeHead = client.Dump_2();
        MappingNode2? node = mappingNodeHead.Value;
        while (node is not null)
        {
            Mapping2 mapping = node.Mapping;
            Console.WriteLine(mapping);
            node = node.Next;
        }

        return 0;
    }

    private static int Get(IPEndPoint ipEndPoint, IReadOnlyList<string> args)
    {
        if ((args.Count != 3) ||
            !int.TryParse(args[0], out int program) ||
            !int.TryParse(args[1], out int version) ||
            !Enum.TryParse(args[2], true, out ProtocolKind protocol))
        {
            PrintUsage();
            return 1;
        }

        using PortMapperClient client = CreateClient(ipEndPoint);
        int result = client.GetPort_2(
            new Mapping2
            {
                ProgramNumber = program,
                VersionNumber = version,
                Protocol = protocol
            });

        Console.WriteLine(result);
        return 0;
    }

    private static int Set(IPEndPoint ipEndPoint, IReadOnlyList<string> args)
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

        using PortMapperClient client = CreateClient(ipEndPoint);
        bool result = client.Set_2(
            new Mapping2
            {
                ProgramNumber = program,
                VersionNumber = version,
                Protocol = protocol,
                Port = port
            });

        Console.WriteLine(result);
        return 0;
    }

    private static int Unset(IPEndPoint ipEndPoint, IReadOnlyList<string> args)
    {
        if (args.Count is < 2 or > 3)
        {
            PrintUsage();
            return 1;
        }

        if (!int.TryParse(args[0], out int program) || !int.TryParse(args[1], out int version))
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

        using PortMapperClient client = CreateClient(ipEndPoint);
        bool result = client.Unset_2(
            new Mapping2
            {
                ProgramNumber = program,
                VersionNumber = version,
                Protocol = protocol
            });

        Console.WriteLine(result);
        return 0;
    }

    private static PortMapperClient CreateClient(IPEndPoint ipEndPoint) =>
        new(Protocol.Tcp, ipEndPoint.Address, ipEndPoint.Port);

    private static void PrintUsage()
    {
        string processName = Process.GetCurrentProcess().ProcessName;
        const string Ip = "[<ip address>[:port]]";
        Console.WriteLine("Usage:");
        Console.WriteLine($"  {processName} dump {Ip}");
        Console.WriteLine($"  {processName} get {Ip} <program> <version> <protocol>");
        Console.WriteLine($"  {processName} set {Ip} <program> <version> <protocol> <port>");
        Console.WriteLine($"  {processName} unset {Ip} <program> <version> [<port>]");
    }

    private enum Command
    {
        Unknown,
        Dump,
        Get,
        Set,
        Unset
    }
}
