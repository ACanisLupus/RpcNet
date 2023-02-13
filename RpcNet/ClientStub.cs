// Copyright by Artur Wolf

namespace RpcNet;

using System.Net;
using Internal;

public abstract class ClientStub : IDisposable
{
    protected readonly XdrVoid Void = new();
    protected readonly ClientSettings Settings;

    private readonly INetworkClient _networkClient;

    protected ClientStub(
        Protocol protocol,
        IPAddress ipAddress,
        int port,
        int program,
        int version,
        ClientSettings clientSettings = default)
    {
        if (ipAddress == null)
        {
            throw new ArgumentNullException(nameof(ipAddress));
        }

        Settings = clientSettings;

        switch (protocol)
        {
            case Protocol.Tcp:
                _networkClient = new RpcTcpClient(ipAddress, port, program, version, clientSettings);
                break;
            case Protocol.Udp:
                _networkClient = new RpcUdpClient(ipAddress, port, program, version, clientSettings);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(protocol));
        }
    }

    public TimeSpan ReceiveTimeout
    {
        get => _networkClient.ReceiveTimeout;
        set => _networkClient.ReceiveTimeout = value;
    }

    public TimeSpan SendTimeout
    {
        get => _networkClient.SendTimeout;
        set => _networkClient.SendTimeout = value;
    }

    public void Call(int procedure, int version, IXdrDataType argument, IXdrDataType result)
    {
        lock (_networkClient)
        {
            _networkClient.Call(procedure, version, argument, result);
        }
    }

    public void Dispose() => _networkClient.Dispose();
}
