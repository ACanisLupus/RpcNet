// Copyright by Artur Wolf

namespace RpcNet;

using System.Net;
using RpcNet.Internal;

public abstract class ClientStub(INetworkClient networkClient, RpcEndPoint rpcEndPoint, ClientSettings settings) : IDisposable
{
    private readonly SemaphoreSlim _lockCall = new(1, 1);

    protected RpcEndPoint RpcEndPoint { get; } = rpcEndPoint;
    protected ClientSettings Settings { get; private set; } = settings;
    protected XdrVoid Void { get; } = new();

    public TimeSpan ReceiveTimeout
    {
        get => networkClient.ReceiveTimeout;
        set => networkClient.ReceiveTimeout = value;
    }

    public TimeSpan SendTimeout
    {
        get => networkClient.SendTimeout;
        set => networkClient.SendTimeout = value;
    }

    protected static INetworkClient Connect(Protocol protocol, IPAddress ipAddress, int port, int programNumber, int versionNumber, ClientSettings clientSettings)
    {
        return protocol switch
        {
            Protocol.Tcp => RpcTcpClient.Connect(ipAddress, port, programNumber, versionNumber, clientSettings),
            Protocol.Udp => RpcUdpClient.Connect(ipAddress, port, programNumber, versionNumber, clientSettings),
            _ => throw new ArgumentOutOfRangeException(nameof(protocol))
        };
    }

    public void Dispose() => networkClient.Dispose();

    protected void Call(int procedure, int version, IXdrDataType argument, IXdrDataType result)
    {
        _lockCall.Wait();
        try
        {
            networkClient.Call(procedure, version, argument, result);
        }
        finally
        {
            _lockCall.Release();
        }
    }
}
