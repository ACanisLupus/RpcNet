namespace Unset
{
    using System;
    using System.Net;
    using RpcNet;
    using RpcNet.PortMapper;

    internal class Program
    {
        private static void Main(string[] args)
        {
            if ((args.Length < 2) || (args.Length > 3))
            {
                Console.Error.WriteLine("Usage: unset <program> <version> [<protocol>]");
                return;
            }

            if (!int.TryParse(args[0], out int program) || !int.TryParse(args[1], out int version))
            {
                Console.Error.WriteLine("Usage: unset <program> <version> [<protocol>]");
                return;
            }

            Protocol protocol = Protocol.Unknown;
            if ((args.Length == 3) && !Enum.TryParse(args[2], true, out protocol))
            {
                Console.Error.WriteLine("Usage: unset <program> <version> [<protocol>]");
                return;
            }

            using var portMapperClient = new PortMapperClient(Protocol.Tcp, IPAddress.Loopback);
            bool result = portMapperClient.Unset(new Mapping
            {
                Program = program,
                Version = version,
                Protocol = protocol
            });

            Console.WriteLine(result);
        }
    }
}
