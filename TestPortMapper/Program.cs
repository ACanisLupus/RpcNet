namespace PortMapper
{
    using System;
    using System.Net;
    using RpcNet;

    internal class Program
    {
        private static void Main()
        {
            {
                using var portMapperServer = new PortMapperServer(IPAddress.Any, new Logger());
                portMapperServer.Start();
                Console.ReadKey(true);
            }

            Console.ReadKey(true);
        }

        private class Logger : ILogger
        {
            public void Error(string entry)
            {
                Console.WriteLine("ERROR " + entry);
            }

            public void Info(string entry)
            {
                Console.WriteLine("INFO  " + entry);
            }

            public void Trace(string entry)
            {
                Console.WriteLine("TRACE " + entry);
            }
        }
    }
}
