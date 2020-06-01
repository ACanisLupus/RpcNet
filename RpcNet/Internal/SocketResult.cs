namespace RpcNet.Internal
{
    using System.Net.Sockets;

    public struct SocketResult
    {
        public SocketError SocketError;
        public int BytesLength;
    }
}
