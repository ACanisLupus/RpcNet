namespace RpcNet.Internal
{
    using System.Net;
    using System.Net.Sockets;

    public struct NetworkResult
    {
        public SocketError SocketError;
        public IPEndPoint IpEndPoint;
    }
}
