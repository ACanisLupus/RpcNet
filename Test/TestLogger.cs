namespace Test
{
    using System;
    using RpcNet;

    internal class TestLogger : ILogger
    {
        private readonly string name;

        public TestLogger(string name)
        {
            this.name = name;
        }

        public void Trace(string entry) => Console.WriteLine($"[{this.name}] [TRACE] {entry}");

        public void Info(string entry) => Console.WriteLine($"[{this.name}] [INFO]  {entry}");

        public void Error(string entry) => Console.WriteLine($"[{this.name}] [ERROR] {entry}");
    }
}
