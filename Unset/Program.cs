namespace Unset
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using RpcNet;
    using RpcNet.PortMapper;

    internal class Program
    {
        private static void ShowUsage() => Console.Error.WriteLine(
            $"Usage: {Process.GetCurrentProcess().ProcessName} <program> <version> [<protocol>]");

        private static int Main(string[] args)
        {
            if ((args.Length < 2) || (args.Length > 3))
            {
                ShowUsage();
                return 1;
            }

            if (!int.TryParse(args[0], out int program) || !int.TryParse(args[1], out int version))
            {
                ShowUsage();
                return 1;
            }

            Protocol protocol = Protocol.Unknown;
            if ((args.Length == 3) && !Enum.TryParse(args[2], true, out protocol))
            {
                ShowUsage();
                return 1;
            }

            using var portMapperClient = new PortMapperClient(Protocol.Tcp, IPAddress.Loopback);
            bool result = portMapperClient.Unset(
                new Mapping
                {
                    Program = program,
                    Version = version,
                    Protocol = protocol
                });

            Console.WriteLine(result);
            return 0;
        }
    }
}
