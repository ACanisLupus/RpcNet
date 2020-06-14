namespace RpcNet.Internal
{
    public interface INetworkClient
    {
        int TimeoutInMilliseconds { get; set; }

        void Call(uint procedure, uint version, IXdrWritable argument, IXdrReadable result);
    }
}
