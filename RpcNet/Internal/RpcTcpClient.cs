// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;
using PortMapper;

// Public for tests
public sealed class RpcTcpClient : INetworkClient
{
    private readonly RpcCall _call;
    private readonly IPEndPoint _remoteIpEndPoint;
    private readonly TcpReader _tcpReader;
    private readonly TcpWriter _tcpWriter;

    private Socket _client;

    public RpcTcpClient(IPAddress ipAddress, int port, int program, int version, ClientSettings? clientSettings = default)
    {
        clientSettings ??= new ClientSettings();

        if (port == 0)
        {
            if (program == PortMapperConstants.PortMapperProgram)
            {
                port = PortMapperConstants.PortMapperPort;
            }
            else
            {
                port = PortMapperUtilities.GetPort(
                    ProtocolKind.Tcp,
                    ipAddress,
                    clientSettings.PortMapperPort,
                    program,
                    version,
                    clientSettings);
            }
        }

        _remoteIpEndPoint = new IPEndPoint(ipAddress, port);
        _client = new Socket(SocketType.Stream, ProtocolType.Tcp);
        ReceiveTimeout = clientSettings.ReceiveTimeout;
        SendTimeout = clientSettings.SendTimeout;
        EstablishConnection();
        _tcpReader = new TcpReader(_client, clientSettings.Logger);
        _tcpWriter = new TcpWriter(_client, clientSettings.Logger);
        _call = new RpcCall(program, _remoteIpEndPoint, _tcpReader, _tcpWriter, ReestablishConnection, clientSettings.Logger);
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

    private void EstablishConnection()
    {
        try
        {
            _client.Connect(_remoteIpEndPoint);
        }
        catch (SocketException e)
        {
            throw new RpcException($"Could not connect to {_remoteIpEndPoint}. Socket error: {e.SocketErrorCode}.");
        }
    }

    private void ReestablishConnection()
    {
        _client.Close();
        _client = new Socket(SocketType.Stream, ProtocolType.Tcp);
        EstablishConnection();
        _tcpReader.Reset(_client);
        _tcpWriter.Reset(_client);
    }
}
