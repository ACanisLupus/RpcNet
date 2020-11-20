namespace PortMapperDump
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using RpcNet;
    using RpcNet.PortMapper;

    internal class Program
    {
        private static int Main(string[] args)
        {
            var ipAddresses = new List<IPAddress>();

            foreach (string arg in args)
            {
                if (!IPAddress.TryParse(arg, out IPAddress ipAddress))
                {
                    Console.Error.WriteLine($"Usage: {Process.GetCurrentProcess().ProcessName} <ip addresses>");
                    return 1;
                }

                ipAddresses.Add(ipAddress);
            }

            if (ipAddresses.Count == 0)
            {
                ipAddresses.Add(IPAddress.Loopback);
            }

            bool firstEntry = true;

            foreach (IPAddress ipAddress in ipAddresses)
            {
                if (firstEntry)
                {
                    firstEntry = false;
                }
                else
                {
                    Console.WriteLine();
                }

                try
                {
                    Console.WriteLine(ipAddress);
                    using var portMapperClient = new PortMapperClient(Protocol.Tcp, ipAddress);
                    IReadOnlyList<Mapping> list = portMapperClient.Dump();
                    foreach (Mapping mapping in list)
                    {
                        Console.WriteLine(
                            $"Protocol: {mapping.Protocol}, Program: {mapping.Program}, " +
                            $"Version: {mapping.Version}, Port: {mapping.Port}");
                    }
                }
                catch (RpcException e)
                {
                    Console.WriteLine($"Could not dump ports. Reason: {e.Message}");
                }
            }

            return 0;
        }
    }
}
