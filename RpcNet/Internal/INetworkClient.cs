namespace RpcNet.Internal
{
    using System;

    // Public for tests
    public interface INetworkClient : IDisposable
    {
        TimeSpan ReceiveTimeout { get; set; }
        TimeSpan SendTimeout { get; set; }

        void Call(int procedure, int version, IXdrWritable argument, IXdrReadable result);
    }
}
