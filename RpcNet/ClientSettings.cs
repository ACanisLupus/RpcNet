// Copyright by Artur Wolf

namespace RpcNet;

using RpcNet.Internal;
using RpcNet.PortMapper;

public sealed class ClientSettings
{
    public static ClientSettings Default { get; } = new();

    public ILogger? Logger { get; init; }
    public int PortMapperPort { get; init; } = PortMapperConstants.PortMapperPort;
    public TimeSpan SendTimeout { get; init; } = Utilities.DefaultClientSendTimeout;
    public TimeSpan ReceiveTimeout { get; init; } = Utilities.DefaultClientReceiveTimeout;
}
