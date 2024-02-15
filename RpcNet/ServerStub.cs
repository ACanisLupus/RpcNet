// Copyright by Artur Wolf

namespace RpcNet;

using System.Net;
using RpcNet.Internal;

public abstract class ServerStub : IDisposable
{
    protected readonly ServerSettings Settings;
    protected readonly XdrVoid Void = new();

    private readonly object _lock = new();
    private readonly RpcTcpServer? _rpcTcpServer;
    private readonly RpcUdpServer? _rpcUdpServer;

    private bool _isDisposed;

    protected ServerStub(Protocol protocol, IPAddress ipAddress, int port, int program, int[] versions, ServerSettings? serverSettings = default)
    {
        if (ipAddress is null)
        {
            throw new ArgumentNullException(nameof(ipAddress));
        }

        if (versions is null || (versions.Length == 0))
        {
            throw new ArgumentNullException(nameof(versions));
        }

        Settings = serverSettings ?? new ServerSettings();

        Action<ReceivedRpcCall> dispatchReceivedCall = DispatchReceivedCallWithLock;
        if (Settings.LockFreeDispatcher)
        {
            dispatchReceivedCall = DispatchReceivedCall;
        }

        if (protocol.HasFlag(Protocol.Tcp))
        {
            _rpcTcpServer = new RpcTcpServer(ipAddress, port, program, versions, dispatchReceivedCall, Settings);
        }

        if (protocol.HasFlag(Protocol.Udp))
        {
            _rpcUdpServer = new RpcUdpServer(ipAddress, port, program, versions, dispatchReceivedCall, Settings);
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

    private void DispatchReceivedCallWithLock(ReceivedRpcCall call)
    {
        lock (_lock)
        {
            DispatchReceivedCall(call);
        }
    }
}
