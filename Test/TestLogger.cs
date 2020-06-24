namespace RpcNet.Test
{
    using System;

    public class TestLogger : ILogger
    {
        public static TestLogger Instance { get; } = new TestLogger();

        /// <inheritdoc />
        public void Trace(string entry) => Console.WriteLine("TRACE " + entry);

        /// <inheritdoc />
        public void Info(string entry) => Console.WriteLine("INFO " + entry);

        /// <inheritdoc />
        public void Error(string entry) => Console.WriteLine("ERROR " + entry);
    }
}
