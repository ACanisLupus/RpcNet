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

    public static RpcTcpClient Connect(IPAddress ipAddress, int port, int program, int version, ClientSettings clientSettings)
    {
        if (port == 0)
        {
            port = program == PortMapperConstants.PortMapperProgram
                ? PortMapperConstants.PortMapperPort
                : PortMapperUtilities.GetPort(ProtocolKind.Tcp, ipAddress, clientSettings.PortMapperPort, program, version, clientSettings);
        }

        Socket socket = new(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        Utilities.SetReceiveTimeout(socket, clientSettings.ReceiveTimeout);
        Utilities.SetSendTimeout(socket, clientSettings.SendTimeout);

        try
        {
            EstablishConnection(socket, ipAddress, port);
        }
        catch (RpcException)
        {
            EstablishConnection(socket, Utilities.GetAlternateIpAddress(ipAddress), port);
        }

        return new RpcTcpClient(socket, ipAddress, port, program);
    }

    public void Call(int procedure, int version, IXdrDataType argument, IXdrDataType result) => _call.SendCall(procedure, version, argument, result);
    public void Dispose() => _socket.Dispose();

    private static void EstablishConnection(Socket socket, IPAddress ipAddress, int port)
    {
        try
        {
            socket.Connect(ipAddress, port);
        }
        catch (SocketException e)
        {
            throw new RpcException($"Could not connect to {ipAddress}:{port}. Socket error code: {e.SocketErrorCode}.");
        }
    }
}
