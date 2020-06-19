namespace RpcNet.Internal
{
    using System.Net;
    using System.Net.Sockets;

    // Public for tests
    public struct NetworkResult
    {
        public SocketError SocketError;
        public IPEndPoint IpEndPoint;
    }
}
