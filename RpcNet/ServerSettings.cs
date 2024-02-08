// Copyright by Artur Wolf

namespace RpcNet;

using RpcNet.Internal;
using RpcNet.PortMapper;

public sealed class ServerSettings
{
    public ILogger? Logger { get; set; }
    public int PortMapperPort { get; set; } = PortMapperConstants.PortMapperPort;
    public TimeSpan SendTimeout { get; set; } = Utilities.DefaultServerSendTimeout;
    public TimeSpan ReceiveTimeout { get; set; } = Utilities.DefaultServerReceiveTimeout;
}
