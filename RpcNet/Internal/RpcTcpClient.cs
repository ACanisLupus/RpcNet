// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;
using RpcNet.PortMapper;

// Public for tests
public sealed class RpcTcpClient : INetworkClient
{
    private readonly RpcCall _call;
    private readonly Socket _socket;
    private readonly ClientSettings _clientSettings;
    private readonly EndPoint _remoteEndPoint;

    public RpcTcpClient(IPAddress ipAddress, int port, int program, int version, ClientSettings clientSettings)
    {
        _clientSettings = clientSettings;

        if (port == 0)
        {
            if (program == PortMapperConstants.PortMapperProgram)
            {
                port = PortMapperConstants.PortMapperPort;
            }
            else
            {
                port = PortMapperUtilities.GetPort(ProtocolKind.Tcp, ipAddress, _clientSettings.PortMapperPort, program, version, clientSettings);
            }
        }

        try
        {
            _remoteEndPoint = new IPEndPoint(ipAddress, port);
            _socket = EstablishConnection();
        }
        catch (RpcException)
        {
            _remoteEndPoint = new IPEndPoint(Utilities.GetAlternateIpAddress(ipAddress), port);
            _socket = EstablishConnection();
        }

        TcpReader tcpReader = new(_socket);
        TcpWriter tcpWriter = new(_socket);
        _call = new RpcCall(program, _remoteEndPoint, tcpReader, tcpWriter);
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

    public void Call(int procedure, int version, IXdrDataType argument, IXdrDataType result) => _call.SendCall(procedure, version, argument, result);
    public void Dispose() => _socket.Dispose();

    private Socket EstablishConnection()
    {
        try
        {
            var socket = new Socket(_remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Utilities.SetReceiveTimeout(socket, _clientSettings.ReceiveTimeout);
            Utilities.SetSendTimeout(socket, _clientSettings.SendTimeout);

            socket.Connect(_remoteEndPoint);

            return socket;
        }
        catch (SocketException e)
        {
            throw new RpcException($"Could not connect to {_remoteEndPoint}. Socket error code: {e.SocketErrorCode}.");
        }
    }
}
