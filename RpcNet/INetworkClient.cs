// Copyright by Artur Wolf

namespace RpcNet;

public interface INetworkClient : IDisposable
{
    TimeSpan ReceiveTimeout { get; set; }
    TimeSpan SendTimeout { get; set; }

    void Call(int procedure, int version, IXdrDataType argument, IXdrDataType result);
}
