// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;
using RpcNet.PortMapper;

internal sealed class RpcUdpClient : INetworkClient
{
    private readonly RpcCall _call;
    private readonly UdpReader _reader;
    private readonly Socket _socket;
    private readonly UdpWriter _writer;

    public RpcUdpClient(Socket socket, IPAddress ipAddress, int port, int program)
    {
        _socket = socket;

        IPEndPoint remoteEndPoint = new(ipAddress, port);
        _reader = new UdpReader(socket);
        _writer = new UdpWriter(socket);
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

        return new RpcUdpClient(socket, ipAddress, port, program)
        {
            ReceiveTimeout = clientSettings.ReceiveTimeout,
            SendTimeout = clientSettings.SendTimeout
        };
    }

    public ValueTask CallAsync(int procedure, int version, IXdrDataType argument, IXdrDataType result, CancellationToken cancellationToken) =>
        _call.SendCallAsync(procedure, version, argument, result, cancellationToken);

    public void Dispose() => _socket.Dispose();
}
