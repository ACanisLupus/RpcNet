// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;
using PortMapper;

// Public for tests
public sealed class RpcTcpClient : INetworkClient
{
    private readonly RpcCall _call;
    private readonly Socket _client;
    private readonly ClientSettings _clientSettings;
    private readonly IPEndPoint _remoteIpEndPoint;
    private readonly TcpReader _tcpReader;
    private readonly TcpWriter _tcpWriter;

    public RpcTcpClient(IPAddress ipAddress, int port, int program, int version, ClientSettings? clientSettings = default)
    {
        _clientSettings = clientSettings ?? new ClientSettings();

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
            _remoteIpEndPoint = new IPEndPoint(ipAddress, port);
            _client = EstablishConnection();
        }
        catch (RpcException)
        {
            _remoteIpEndPoint = new IPEndPoint(Utilities.GetAlternateIpAddress(ipAddress), port);
            _client = EstablishConnection();
        }

        _tcpReader = new TcpReader(_client, _clientSettings.Logger);
        _tcpWriter = new TcpWriter(_client, _clientSettings.Logger);
        _call = new RpcCall(program, _remoteIpEndPoint, _tcpReader, _tcpWriter);
    }

    public TimeSpan ReceiveTimeout
    {
        get => Utilities.GetReceiveTimeout(_client);
        set => Utilities.SetReceiveTimeout(_client, value);
    }

    public TimeSpan SendTimeout
    {
        get => Utilities.GetSendTimeout(_client);
        set => Utilities.SetSendTimeout(_client, value);
    }

    public void Call(int procedure, int version, IXdrDataType argument, IXdrDataType result) => _call.SendCall(procedure, version, argument, result);
    public void Dispose() => _client.Dispose();

    private Socket EstablishConnection()
    {
        try
        {
            var client = new Socket(_remoteIpEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Utilities.SetReceiveTimeout(client, _clientSettings.ReceiveTimeout);
            Utilities.SetSendTimeout(client, _clientSettings.SendTimeout);

            client.Connect(_remoteIpEndPoint);

            return client;
        }
        catch (SocketException e)
        {
            throw new RpcException($"Could not connect to {_remoteIpEndPoint}. Socket error code: {e.SocketErrorCode}.");
        }
    }
}
