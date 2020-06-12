namespace RpcNet.Internal
{
    using System;
    using System.Net.Sockets;
    using System.Threading;

    public class TcpBufferWriter : INetworkWriter
    {
        private const int TcpHeader = 4;
        private readonly Socket socket;
        private readonly byte[] buffer;
        private int writeIndex;

        public TcpBufferWriter(Socket socket) : this(socket, 65536)
        {
        }

        public TcpBufferWriter(Socket socket, int bufferSize)
        {
            if (bufferSize < 2 * sizeof(int) || bufferSize % 4 != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }

            this.socket = socket;
            this.buffer = new byte[bufferSize];
        }

        public void BeginWriting() => this.writeIndex = TcpHeader;
        public void EndWriting() => this.FlushPacket(true);

        public Span<byte> Reserve(int length)
        {
            int maxLength = this.buffer.Length - this.writeIndex;

            // Integers (4 bytes) and padding bytes (> 1 and < 4 bytes) must not be sent fragmented
            if (maxLength < length && maxLength < sizeof(int))
            {
                this.FlushPacket(false);
                maxLength = this.buffer.Length - this.writeIndex;
            }

            int reservedLength = Math.Min(length, maxLength);

            Span<byte> span = this.buffer.AsSpan(this.writeIndex, reservedLength);
            this.writeIndex += reservedLength;
            return span;
        }

        private void FlushPacket(bool lastPacket)
        {
            int length = this.writeIndex - TcpHeader;

            // Last fragment sets first bit to 1
            int lengthToDecode = lastPacket ? length | unchecked((int)0x80000000) : length;

            Utilities.WriteBytesBigEndian(this.buffer.AsSpan(), lengthToDecode);
            this.socket.Send(this.buffer, 0, length + TcpHeader, SocketFlags.None, out SocketError socketError);
            if (socketError != SocketError.Success)
            {
                throw new RpcException($"Could not send packet. Socket error code: {socketError}.");
            }

            // For test purposes. This simulates a slow network
            //for (int i = 0; i < length + TcpHeader; i++)
            //{
            //    byte[] tmpBuffer = new byte[1] { this.buffer[i] };
            //    this.socket.Send(tmpBuffer, 0, 1, SocketFlags.None, out SocketError socketError);
            //    if (socketError != SocketError.Success)
            //    {
            //        throw new RpcException($"Could not send packet. Socket error code: {socketError}.");
            //    }
            //    Thread.Sleep(1);
            //}

            this.BeginWriting();
        }
    }
}
