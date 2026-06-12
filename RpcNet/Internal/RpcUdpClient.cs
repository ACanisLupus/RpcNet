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

    public static RpcUdpClient Connect(IPAddress ipAddress, int port, int program, int version, ClientSettings clientSettings)
    {
        if (port == 0)
        {
            port = program == PortMapperConstants.PortMapperProgram
                ? PortMapperConstants.PortMapperPort
                : PortMapperUtilities.GetPort(ProtocolKind.Udp, ipAddress, clientSettings.PortMapperPort, program, version, clientSettings);
        }

        Socket socket = new(ipAddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        Utilities.FixUdpSocket(socket);

        Utilities.SetReceiveTimeout(socket, clientSettings.ReceiveTimeout);
        Utilities.SetSendTimeout(socket, clientSettings.SendTimeout);

        return new RpcUdpClient(socket, ipAddress, port, program);
    }

    public void Call(int procedure, int version, IXdrDataType argument, IXdrDataType result) =>
        _call.SendCall(procedure, version, argument, result);

    public void Dispose() => _socket.Dispose();
}
