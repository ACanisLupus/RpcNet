namespace RpcNet.Internal
{
    using System.Net.Sockets;

    // Public for tests
    public readonly struct NetworkWriteResult
    {
        public NetworkWriteResult(SocketError socketError)
        {
            this.SocketError = socketError;
        }

        public SocketError SocketError { get; }
        public bool HasError => this.SocketError != SocketError.Success;
    }
}
