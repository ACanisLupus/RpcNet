// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;
using RpcNet.PortMapper;

internal sealed class RpcTcpClient : INetworkClient
{
    private readonly RpcCall _call;
    private readonly TcpReader _reader;
    private readonly Socket _socket;
    private readonly TcpWriter _writer;

    private RpcTcpClient(Socket socket, IPAddress ipAddress, int port, int program)
    {
        _socket = socket;

        IPEndPoint remoteEndPoint = new(ipAddress, port);
        _reader = new TcpReader(_socket);
        _writer = new TcpWriter(_socket);
        _call = new RpcCall(program, remoteEndPoint, _reader, _writer);
    }

    public TimeSpan ReceiveTimeout
    {
        get => _reader.Timeout;
        set => _reader.Timeout = value;
    }

    public TimeSpan SendTimeout
    {
        get => _writer.Timeout;
        set => _writer.Timeout = value;
    }

    public static async ValueTask<RpcTcpClient> ConnectAsync(
        IPAddress ipAddress,
        int port,
        int program,
        int version,
        ClientSettings clientSettings,
        CancellationToken cancellationToken)
    {
        if (port == 0)
        {
            port = program == PortMapperConstants.PortMapperProgram
                ? PortMapperConstants.PortMapperPort
                : await PortMapperUtilities.GetPortAsync(
                        ProtocolKind.Tcp,
                        ipAddress,
                        clientSettings.PortMapperPort,
                        program,
                        version,
                        clientSettings,
                        cancellationToken)
                    .ConfigureAwait(false);
        }

        Socket socket = new(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            await EstablishConnectionAsync(socket, ipAddress, port, cancellationToken).ConfigureAwait(false);
        }
        catch (RpcException)
        {
            await EstablishConnectionAsync(socket, Utilities.GetAlternateIpAddress(ipAddress), port, cancellationToken).ConfigureAwait(false);
        }

        return new RpcTcpClient(socket, ipAddress, port, program)
        {
            ReceiveTimeout = clientSettings.ReceiveTimeout,
            SendTimeout = clientSettings.SendTimeout
        };
    }

    public ValueTask CallAsync(int procedure, int version, IXdrDataType argument, IXdrDataType result, CancellationToken cancellationToken) =>
        _call.SendCallAsync(procedure, version, argument, result, cancellationToken);

    public void Dispose() => _socket.Dispose();

    private static async ValueTask EstablishConnectionAsync(Socket socket, IPAddress ipAddress, int port, CancellationToken cancellationToken)
    {
        try
        {
            await socket.ConnectAsync(ipAddress, port, cancellationToken).ConfigureAwait(false);
        }
        catch (SocketException e)
        {
            throw new RpcException($"Could not connect to {ipAddress}:{port}. Socket error code: {e.SocketErrorCode}.");
        }
    }
}
