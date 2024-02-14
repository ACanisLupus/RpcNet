// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;
using RpcNet.PortMapper;

// Public for tests
public sealed class RpcUdpClient : INetworkClient
{
    private readonly RpcCall _call;
    private readonly Socket _socket;

    public RpcUdpClient(IPAddress ipAddress, int port, int program, int version, ClientSettings clientSettings)
    {
        if (port == 0)
        {
            port = program == PortMapperConstants.PortMapperProgram
                ? PortMapperConstants.PortMapperPort
                : PortMapperUtilities.GetPort(ProtocolKind.Udp, ipAddress, clientSettings.PortMapperPort, program, version, clientSettings);
        }

        var remoteIpEndPoint = new IPEndPoint(ipAddress, port);
        _socket = new Socket(ipAddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        Utilities.FixUdpSocket(_socket);

        ReceiveTimeout = clientSettings.ReceiveTimeout;
        SendTimeout = clientSettings.SendTimeout;

        var reader = new UdpReader(_socket);
        var writer = new UdpWriter(_socket);
        _call = new RpcCall(program, remoteIpEndPoint, reader, writer);
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

    public void Call(int procedure, int version, IXdrDataType argument, IXdrDataType result) =>
        _call.SendCall(procedure, version, argument, result);

    public void Dispose() => _socket.Dispose();
}
