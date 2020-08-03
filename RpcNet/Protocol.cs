namespace RpcNet
{
    using System;

    [Flags]
    public enum Protocol
    {
        Unknown = 0,
        Tcp = 1,
        Udp = 2,
        TcpAndUdp = Tcp | Udp
    }
}
