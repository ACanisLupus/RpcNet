namespace RpcNet.Internal
{
    using System;

    // Public for tests
    public interface INetworkClient : IDisposable
    {
        int TimeoutInMilliseconds { get; set; }

        void Call(int procedure, int version, IXdrWritable argument, IXdrReadable result);
    }
}
