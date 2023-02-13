// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;
using PortMapper;

// Public for tests
public class RpcUdpClient : INetworkClient
{
    private readonly RpcCall _call;
    private readonly Socket _client;

    public RpcUdpClient(
        IPAddress ipAddress,
        int port,
        int program,
        int version,
        ClientSettings? clientSettings = default)
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
                    ProtocolKind.Udp,
                    ipAddress,
                    clientSettings.PortMapperPort,
                    program,
                    version,
                    clientSettings);
            }
        }

        var remoteIpEndPoint = new IPEndPoint(ipAddress, port);
        _client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        ReceiveTimeout = clientSettings.ReceiveTimeout;
        SendTimeout = clientSettings.SendTimeout;
        var reader = new UdpReader(_client);
        var writer = new UdpWriter(_client);
        _call = new RpcCall(program, remoteIpEndPoint, reader, writer, null, clientSettings.Logger);
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

    public void Call(int procedure, int version, IXdrDataType argument, IXdrDataType result) =>
        _call.SendCall(procedure, version, argument, result);

    public void Dispose() => _client.Dispose();
}
