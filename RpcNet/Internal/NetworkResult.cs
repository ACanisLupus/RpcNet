namespace RpcNet.Internal
{
    using System.Net;
    using System.Net.Sockets;

    public struct NetworkResult
    {
        public IPEndPoint RemoteIpEndPoint;
        public SocketError SocketError;
    }
}
