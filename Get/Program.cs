namespace Get
{
    using System;
    using System.Net;
    using RpcNet;
    using RpcNet.PortMapper;

    internal class Program
    {
        private static void Main(string[] args)
        {
            if ((args.Length != 3) ||
                !int.TryParse(args[0], out int program) ||
                !int.TryParse(args[1], out int version) ||
                !Enum.TryParse(args[2], true, out Protocol protocol))
            {
                Console.Error.WriteLine("Usage: get <program> <version> <protocol>");
                return;
            }

            using var portMapperClient = new PortMapperClient(Protocol.Tcp, IPAddress.Loopback);
            int result = portMapperClient.GetPort(new Mapping
            {
                Program = program,
                Version = version,
                Protocol = protocol
            });

            Console.WriteLine(result);
        }
    }
}
