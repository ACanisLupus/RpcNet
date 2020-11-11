namespace RpcNet.Internal
{
    using System.Net;
    using System.Net.Sockets;

    // Public for tests
    public readonly struct NetworkReadResult
    {
        private NetworkReadResult(IPEndPoint remoteIpEndPoint, SocketError socketError, bool isDisconnected)
        {
            this.RemoteIpEndPoint = remoteIpEndPoint;
            this.SocketError = socketError;
            this.IsDisconnected = isDisconnected;
        }

        public IPEndPoint RemoteIpEndPoint { get; }
        public SocketError SocketError { get; }
        public bool IsDisconnected { get; }
        public bool HasError => this.SocketError != SocketError.Success;

        public static NetworkReadResult CreateError(SocketError socketError) =>
            new NetworkReadResult(null, socketError, false);

        public static NetworkReadResult CreateSuccess(IPEndPoint remoteIpEndPoint = default) =>
            new NetworkReadResult(remoteIpEndPoint, SocketError.Success, false);

        public static NetworkReadResult CreateDisconnected() => new NetworkReadResult(null, SocketError.Success, true);
    }
}
