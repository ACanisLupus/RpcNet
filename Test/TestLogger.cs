namespace RpcNet.Test
{
    using System;

    internal class TestLogger : ILogger
    {
        public static TestLogger Instance { get; } = new TestLogger();

        public void Trace(string entry)
        {
            Console.WriteLine("TRACE " + entry);
        }

        public void Info(string entry)
        {
            Console.WriteLine("INFO " + entry);
        }

        public void Error(string entry)
        {
            Console.WriteLine("ERROR " + entry);
        }
    }
}
