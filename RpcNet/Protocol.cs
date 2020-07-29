namespace RpcNet
{
    using System;

    [Flags]
    public enum Protocol
    {
        Tcp = 1,
        Udp = 2,
        TcpAndUdp = Tcp | Udp
    }
}
