namespace RpcNet
{
    using System;
    using RpcNet.Internal;

    public class ServerSettings
    {
        public ILogger Logger { get; set; }
        public int Port { get; set; }
        public int PortMapperPort { get; set; } = PortMapperConstants.PortMapperPort;
        public TimeSpan SendTimeout { get; set; } = Utilities.DefaultServerSendTimeout;
        public TimeSpan ReceiveTimeout { get; set; } = Utilities.DefaultServerReceiveTimeout;
    }
}
