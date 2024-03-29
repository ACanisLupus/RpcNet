// Copyright by Artur Wolf

namespace RpcNet;

using RpcNet.Internal;
using RpcNet.PortMapper;

public sealed class ClientSettings
{
    public static ClientSettings Default { get; } = new ClientSettings();

    public ILogger? Logger { get; set; }
    public int PortMapperPort { get; set; } = PortMapperConstants.PortMapperPort;
    public TimeSpan SendTimeout { get; set; } = Utilities.DefaultClientSendTimeout;
    public TimeSpan ReceiveTimeout { get; set; } = Utilities.DefaultClientReceiveTimeout;
}
