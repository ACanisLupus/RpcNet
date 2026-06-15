// Copyright by Artur Wolf

namespace RpcNet;

using RpcNet.Internal;
using RpcNet.PortMapper;

public sealed class ServerSettings
{
    public static ServerSettings Default { get; } = new();

    public ILogger? Logger { get; init; }
    public int PortMapperPort { get; init; } = PortMapperConstants.PortMapperPort;
    public TimeSpan SendTimeout { get; init; } = Utilities.DefaultServerSendTimeout;
    public TimeSpan ReceiveTimeout { get; init; } = Utilities.DefaultServerReceiveTimeout;

    // Number of UDP datagrams that can be received and processed in parallel on the shared socket.
    public int UdpConcurrency { get; init; } = Environment.ProcessorCount;
}
