namespace RpcNet.PortMapper
{
    using System;
    using RpcNet.Internal;

    public class PortMapperClientSettings
    {
        public ILogger Logger { get; set; }
        public int Port { get; set; } = PortMapperConstants.PortMapperPort;
        public TimeSpan SendTimeout { get; set; } = Utilities.DefaultClientSendTimeout;
        public TimeSpan ReceiveTimeout { get; set; } = Utilities.DefaultClientReceiveTimeout;
    }
}
