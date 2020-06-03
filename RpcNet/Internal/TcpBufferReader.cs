namespace RpcNet.Internal
{
    using System;
    using System.Net.Sockets;

    public class TcpBufferReader : INetworkReader
    {
        private const int TcpHeader = 4;
        private readonly Socket socket;
        private readonly byte[] buffer;
        private int readIndex;

        public TcpBufferReader(Socket socket) : this(socket, 65536)
        {
        }

        public TcpBufferReader(Socket socket, int bufferSize)
        {
            this.socket = socket;
            this.buffer = new byte[bufferSize];
        }

        public SocketError BeginReading()
        {
            while (this.readIndex < 4)
            {
                int length = this.socket.Receive(this.buffer, 0, this.buffer.Length, SocketFlags.None, out SocketError socketError);
                if (socketError != SocketError.Success)
                {
                    return socketError;
                }

                if (length == 0)
                {
                    return SocketError.NotConnected;
                }

                this.readIndex += length;
            }

            return SocketError.Success;
        }

        public void EndReading()
        {
            this.readIndex = 0;
        }

        public ReadOnlySpan<byte> Read(int length)
        {
            return this.buffer.AsSpan();
        }
    }
}
