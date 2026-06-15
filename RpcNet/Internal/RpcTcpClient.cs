// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;
using RpcNet.PortMapper;

internal sealed class RpcTcpClient : INetworkClient
{
    private readonly RpcCall _call;
    private readonly Socket _socket;

    private RpcTcpClient(Socket socket, IPAddress ipAddress, int port, int program)
    {
        _socket = socket;

        IPEndPoint remoteEndPoint = new(ipAddress, port);
        TcpReader tcpReader = new(_socket);
        TcpWriter tcpWriter = new(_socket);
        _call = new RpcCall(program, remoteEndPoint, tcpReader, tcpWriter);
    }

    public TimeSpan ReceiveTimeout
    {
        get => Utilities.GetReceiveTimeout(_socket);
        set => Utilities.SetReceiveTimeout(_socket, value);
    }

    public TimeSpan SendTimeout
    {
        get => Utilities.GetSendTimeout(_socket);
        set => Utilities.SetSendTimeout(_socket, value);
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

        Utilities.SetReceiveTimeout(socket, clientSettings.ReceiveTimeout);
        Utilities.SetSendTimeout(socket, clientSettings.SendTimeout);

        try
        {
            await EstablishConnectionAsync(socket, ipAddress, port, cancellationToken).ConfigureAwait(false);
        }
        catch (RpcException)
        {
            await EstablishConnectionAsync(socket, Utilities.GetAlternateIpAddress(ipAddress), port, cancellationToken).ConfigureAwait(false);
        }

        return new RpcTcpClient(socket, ipAddress, port, program);
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
