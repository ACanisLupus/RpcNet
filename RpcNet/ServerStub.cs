// Copyright by Artur Wolf

namespace RpcNet;

using System.Net;
using Internal;

public abstract class ServerStub : IDisposable
{
    protected readonly XdrVoid Void = new();
    protected readonly ServerSettings Settings;

    private readonly RpcTcpServer _rpcTcpServer;
    private readonly RpcUdpServer _rpcUdpServer;

    private bool _isDisposed;

    protected ServerStub(
        Protocol protocol,
        IPAddress ipAddress,
        int port,
        int program,
        int[] versions,
        ServerSettings serverSettings = default)
    {
        if (ipAddress == null)
        {
            throw new ArgumentNullException(nameof(ipAddress));
        }

        if ((versions == null) || (versions.Length == 0))
        {
            throw new ArgumentNullException(nameof(versions));
        }

        Settings = serverSettings;

        if (protocol.HasFlag(Protocol.Tcp))
        {
            _rpcTcpServer = new RpcTcpServer(ipAddress, port, program, versions, DispatchReceivedCall, serverSettings);
        }

        if (protocol.HasFlag(Protocol.Udp))
        {
            _rpcUdpServer = new RpcUdpServer(ipAddress, port, program, versions, DispatchReceivedCall, serverSettings);
        }
    }

    public int TcpPort { get; private set; }
    public int UdpPort { get; private set; }

    public void Start()
    {
        TcpPort = _rpcTcpServer?.Start() ?? 0;
        UdpPort = _rpcUdpServer?.Start() ?? 0;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected abstract void DispatchReceivedCall(ReceivedRpcCall call);

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _rpcUdpServer?.Dispose();
                _rpcTcpServer?.Dispose();
            }

            _isDisposed = true;
        }
    }
}
