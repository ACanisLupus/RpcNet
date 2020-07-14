namespace RpcNet
{
    using System;

    [Flags]
    public enum Protocols
    {
        TcpOnly = 1,
        UdpOnly = 2,
        TcpAndUdp = TcpOnly | UdpOnly
    }
}
