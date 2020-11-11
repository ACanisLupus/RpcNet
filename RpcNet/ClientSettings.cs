namespace RpcNet
{
    using System;
    using RpcNet.Internal;

    public class ClientSettings
    {
        public ILogger Logger { get; set; }
        public int Port { get; set; }
        public int PortMapperPort { get; set; } = PortMapperConstants.PortMapperPort;
        public TimeSpan SendTimeout { get; set; } = Utilities.DefaultClientSendTimeout;
        public TimeSpan ReceiveTimeout { get; set; } = Utilities.DefaultClientReceiveTimeout;
    }
}
