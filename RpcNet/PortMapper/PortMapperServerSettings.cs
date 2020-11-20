namespace RpcNet.PortMapper
{
    using System;
    using RpcNet.Internal;

    public class PortMapperServerSettings
    {
        public ILogger Logger { get; set; }
        public int Port { get; set; } = PortMapperConstants.PortMapperPort;
        public TimeSpan SendTimeout { get; set; } = Utilities.DefaultServerSendTimeout;
        public TimeSpan ReceiveTimeout { get; set; } = Utilities.DefaultServerReceiveTimeout;
    }
}
