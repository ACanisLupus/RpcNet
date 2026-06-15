// Copyright by Artur Wolf

namespace RpcNet;

using System.Net;
using RpcNet.Internal;

public abstract class ClientStub(INetworkClient networkClient, RpcEndPoint rpcEndPoint, ClientSettings settings) : IDisposable
{
    private readonly SemaphoreSlim _lockCall = new(1, 1);

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

    protected RpcEndPoint RpcEndPoint { get; } = rpcEndPoint;
    protected ClientSettings Settings { get; private set; } = settings;
    protected XdrVoid Void { get; } = new();

    public void Dispose() => networkClient.Dispose();

    protected static async ValueTask<INetworkClient> ConnectAsync(
        Protocol protocol,
        IPAddress ipAddress,
        int port,
        int programNumber,
        int versionNumber,
        ClientSettings clientSettings,
        CancellationToken cancellationToken)
    {
        return protocol switch
        {
            Protocol.Tcp => await RpcTcpClient.ConnectAsync(ipAddress, port, programNumber, versionNumber, clientSettings, cancellationToken)
                .ConfigureAwait(false),
            Protocol.Udp => await RpcUdpClient.ConnectAsync(ipAddress, port, programNumber, versionNumber, clientSettings, cancellationToken)
                .ConfigureAwait(false),
            _ => throw new ArgumentOutOfRangeException(nameof(protocol))
        };
    }

    protected async ValueTask CallAsync(int procedure, int version, IXdrDataType argument, IXdrDataType result, CancellationToken cancellationToken)
    {
        await _lockCall.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await networkClient.CallAsync(procedure, version, argument, result, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _lockCall.Release();
        }
    }
}
