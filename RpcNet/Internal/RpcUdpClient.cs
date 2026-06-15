// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;
using RpcNet.PortMapper;

internal sealed class RpcUdpClient : INetworkClient
{
    private readonly RpcCall _call;
    private readonly Socket _socket;

    public RpcUdpClient(Socket socket, IPAddress ipAddress, int port, int program)
    {
        _socket = socket;

        IPEndPoint remoteEndPoint = new(ipAddress, port);
        UdpReader reader = new(socket);
        UdpWriter writer = new(socket);
        _call = new RpcCall(program, remoteEndPoint, reader, writer);
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

    public static async ValueTask<RpcUdpClient> ConnectAsync(
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
                        ProtocolKind.Udp,
                        ipAddress,
                        clientSettings.PortMapperPort,
                        program,
                        version,
                        clientSettings,
                        cancellationToken)
                    .ConfigureAwait(false);
        }

        Socket socket = new(ipAddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        Utilities.FixUdpSocket(socket);

        Utilities.SetReceiveTimeout(socket, clientSettings.ReceiveTimeout);
        Utilities.SetSendTimeout(socket, clientSettings.SendTimeout);

        return new RpcUdpClient(socket, ipAddress, port, program);
    }

    public ValueTask CallAsync(int procedure, int version, IXdrDataType argument, IXdrDataType result, CancellationToken cancellationToken) =>
        _call.SendCallAsync(procedure, version, argument, result, cancellationToken);

    public void Dispose() => _socket.Dispose();
}
