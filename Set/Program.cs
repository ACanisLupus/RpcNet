namespace Set
{
    using System;
    using System.Net;
    using RpcNet;
    using RpcNet.PortMapper;

    internal class Program
    {
        private static void Main(string[] args)
        {
            if ((args.Length != 4) ||
                !int.TryParse(args[0], out int program) ||
                !int.TryParse(args[1], out int version) ||
                !Enum.TryParse(args[2], true, out Protocol protocol) ||
                !int.TryParse(args[3], out int port))
            {
                Console.Error.WriteLine("Usage: set <program> <version> <protocol> <port>");
                return;
            }

            using var portMapperClient = new PortMapperClient(Protocol.Tcp, IPAddress.Loopback);
            bool result = portMapperClient.Set(new Mapping
            {
                Program = program,
                Version = version,
                Protocol = protocol,
                Port =  port
            });

            Console.WriteLine(result);
        }
    }
}
