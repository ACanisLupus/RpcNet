using System;

namespace RpcNet.Internal
{
    public interface INetworkClient : IDisposable
    {
        int TimeoutInMilliseconds { get; set; }

        void Call(int procedure, int version, IXdrWritable argument, IXdrReadable result);
    }
}
