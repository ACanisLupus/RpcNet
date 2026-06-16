// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;
using RpcNet.PortMapper;

internal sealed class RpcUdpServer : IAsyncDisposable
{
    private readonly IPAddress _ipAddress;
    private readonly int _program;
    private readonly Func<ReceivedRpcCall, CancellationToken, ValueTask> _receivedCallDispatcher;
    private readonly ServerSettings _serverSettings;
    private readonly Socket _socket;
    private readonly int[] _versions;

    private bool _isDisposed;
    private int _port;
    private Task[]? _receivingTasks;
    private volatile bool _stopReceiving;

    public RpcUdpServer(
        IPAddress ipAddress,
        int port,
        int program,
        int[] versions,
        Func<ReceivedRpcCall, CancellationToken, ValueTask> receivedCallDispatcher,
        ServerSettings serverSettings)
    {
        _serverSettings = serverSettings;
        _program = program;
        _versions = versions;
        _receivedCallDispatcher = receivedCallDispatcher;
        _ipAddress = ipAddress;
        _port = port;
        _socket = new Socket(ipAddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        Utilities.FixUdpSocket(_socket);

        if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
            try
            {
                _socket.DualMode = true;
            }
            catch (SocketException e)
            {
                serverSettings.Logger?.Error($"Could not enable dual mode. Socket error code: {e.SocketErrorCode}. Only IPv6 is available.");
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        _stopReceiving = true;
        try
        {
            // Necessary for Linux. Dispose doesn't cancel synchronous calls
            _socket.Shutdown(SocketShutdown.Both);
        }
        catch
        {
            // Ignored
        }

        _socket.Dispose();

        if (_receivingTasks is not null)
        {
            foreach (Task receivingTask in _receivingTasks)
            {
                try
                {
                    await receivingTask.ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    _serverSettings.Logger?.Error($"The following error occurred while waiting for a receiving task to finish: {e}");
                }
            }
        }

        _isDisposed = true;
    }

    public async Task<int> StartAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, nameof(RpcUdpServer));

        if (_receivingTasks is not null)
        {
            return _port;
        }

        try
        {
            _socket.Bind(new IPEndPoint(_ipAddress, _port));
        }
        catch (SocketException e)
        {
            throw new RpcException($"Could not start UDP listener. Socket error code: {e.SocketErrorCode}.");
        }

        if (_socket.LocalEndPoint is not IPEndPoint localEndPoint)
        {
            throw new InvalidOperationException("Could not find local endpoint for server socket.");
        }

        if (_port == 0)
        {
            _port = localEndPoint.Port;
        }

        if ((_program != PortMapperConstants.PortMapperProgram) && (_serverSettings.PortMapperPort != 0))
        {
            ClientSettings clientSettings = new()
            {
                Logger = _serverSettings.Logger,
                ReceiveTimeout = _serverSettings.ReceiveTimeout,
                SendTimeout = _serverSettings.SendTimeout
            };
            foreach (int version in _versions)
            {
                await PortMapperUtilities.UnsetAndSetPortAsync(
                        _ipAddress.AddressFamily,
                        ProtocolKind.Udp,
                        _serverSettings.PortMapperPort,
                        _port,
                        _program,
                        version,
                        clientSettings,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        _serverSettings.Logger?.Info($"UDP Server listening on {localEndPoint}...");

        int concurrency = Math.Max(1, _serverSettings.UdpConcurrency);
        _receivingTasks = new Task[concurrency];
        for (int i = 0; i < concurrency; i++)
        {
            _receivingTasks[i] = Task.Run(() => HandlingUdpCallsAsync(cancellationToken), cancellationToken);
        }

        return _port;
    }

    private async Task HandlingUdpCallsAsync(CancellationToken cancellationToken)
    {
        // Each receive loop owns its reader, writer and call state so that multiple UDP
        // datagrams can be received and processed in parallel on the shared socket.
        UdpReader reader = new(_socket);
        UdpWriter writer = new(_socket);
        ReceivedRpcCall receivedCall = new(_program, _versions, reader, writer, _receivedCallDispatcher);

        while (!_stopReceiving)
        {
            try
            {
                EndPoint remoteEndPoint = await reader.BeginReadingAsync(cancellationToken).ConfigureAwait(false);

                writer.BeginWriting();
                await receivedCall.HandleCallAsync(new RpcEndPoint(remoteEndPoint, Protocol.Udp), cancellationToken).ConfigureAwait(false);
                reader.EndReading();
                await writer.EndWritingAsync(remoteEndPoint, cancellationToken).ConfigureAwait(false);
            }
            catch (RpcException e)
            {
                if (!_stopReceiving)
                {
                    _serverSettings.Logger?.Error(e.Message);
                }
            }
            catch (Exception e)
            {
                if (!_stopReceiving)
                {
                    _serverSettings.Logger?.Error($"The following error occurred while processing UDP call: {e}");
                }
            }
        }
    }
}
