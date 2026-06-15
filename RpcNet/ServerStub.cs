// Copyright by Artur Wolf

namespace RpcNet;

using System.Net;
using RpcNet.Internal;

public abstract class ServerStub : IAsyncDisposable
{
    protected readonly ServerSettings Settings;
    protected readonly XdrVoid Void = new();

    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly RpcTcpServer? _rpcTcpServer;
    private readonly RpcUdpServer? _rpcUdpServer;

    private bool _isDisposed;

    protected ServerStub(Protocol protocol, IPAddress ipAddress, int port, int program, int[] versions, ServerSettings? serverSettings = null)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        if (versions is null || (versions.Length == 0))
        {
            throw new ArgumentNullException(nameof(versions));
        }

        Settings = serverSettings ?? new ServerSettings();

        Func<ReceivedRpcCall, CancellationToken, ValueTask> dispatchReceivedCall = DispatchReceivedCallWithLock;
        if (Settings.LockFreeDispatcher)
        {
            dispatchReceivedCall = DispatchReceivedCallAsync;
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

    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(true).ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    public async ValueTask StartAsync(CancellationToken cancellationToken)
    {
        TcpPort = _rpcTcpServer is not null ? await _rpcTcpServer.StartAsync(cancellationToken).ConfigureAwait(false) : 0;
        UdpPort = _rpcUdpServer is not null ? await _rpcUdpServer.StartAsync(cancellationToken).ConfigureAwait(false) : 0;
    }

    protected abstract ValueTask DispatchReceivedCallAsync(ReceivedRpcCall call, CancellationToken cancellationToken);

    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                if (_rpcUdpServer is not null)
                {
                    await _rpcUdpServer.DisposeAsync().ConfigureAwait(false);
                }

                if (_rpcTcpServer is not null)
                {
                    await _rpcTcpServer.DisposeAsync().ConfigureAwait(false);
                }
            }

            _isDisposed = true;
        }
    }

    private async ValueTask DispatchReceivedCallWithLock(ReceivedRpcCall call, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await DispatchReceivedCallAsync(call, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _lock.Release();
        }
    }
}
