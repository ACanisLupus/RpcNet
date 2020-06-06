namespace RpcNet.Internal
{
    using System;
    using System.Net.Sockets;

    public class TcpBufferReader : INetworkReader
    {
        private const int TcpHeaderLength = 4;
        private readonly Socket socket;
        private readonly byte[] buffer;
        private int readIndex;
        private int writeIndex;
        private bool lastFragment;
        private int unreceivedBytesForCurrentFragment;
        private int endIndexOfCurrentFragment;

        public TcpBufferReader(Socket socket) : this(socket, 65536)
        {
        }

        public TcpBufferReader(Socket socket, int bufferSize)
        {
            if (bufferSize < 2 * sizeof(int) || bufferSize % 4 != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }

            this.socket = socket;
            this.buffer = new byte[bufferSize];
        }

        public bool BeginReading(out SocketError socketError)
        {
            return this.FillBuffer(out socketError);
        }

        public void EndReading()
        {
            this.readIndex = 0;
            this.writeIndex = 0;
            this.lastFragment = false;
            this.endIndexOfCurrentFragment = 0;
        }

        public ReadOnlySpan<byte> Read(int length)
        {
            if (!this.FillBuffer(out SocketError socketError))
            {
                throw new RpcException($"Could not receive from TCP stream. Socket error code: {socketError}.");
            }

            int endIndex = Math.Min(this.endIndexOfCurrentFragment, this.buffer.Length);
            int availableBytes = endIndex - this.readIndex;
            int bytesToRead = Math.Min(availableBytes, length);

            Span<byte> span = this.buffer.AsSpan(this.readIndex, bytesToRead);
            this.readIndex += bytesToRead;
            return span;
        }

        // Maybe there are 1000 implementations better than this one. But its working...
        private bool FillBuffer(out SocketError socketError)
        {
            if (this.endIndexOfCurrentFragment == this.readIndex && this.writeIndex > this.readIndex)
            {
                this.ReadFragmentLength();
            }

            if (this.lastFragment && this.unreceivedBytesForCurrentFragment == 0)
            {
                // This message is complete
                socketError = SocketError.Success;
                return true;
            }

            if (this.writeIndex == this.buffer.Length || this.unreceivedBytesForCurrentFragment < 0)
            {
                if (this.readIndex < this.writeIndex)
                {
                    // No place to write. Wait for more reads
                    socketError = SocketError.Success;
                    return true;
                }

                // Buffer is empty. Prepare for receive
                this.endIndexOfCurrentFragment -= this.writeIndex;
                this.readIndex = 0;
                this.writeIndex = 0;
            }

            if (this.unreceivedBytesForCurrentFragment > 0)
            {
                // Still data missing from current fragment
                int bytesToReceive = Math.Min(this.unreceivedBytesForCurrentFragment, this.buffer.Length - this.writeIndex);
                if (!this.ReceiveAtLeast(bytesToReceive, out socketError))
                {
                    return false;
                }

                this.unreceivedBytesForCurrentFragment -= bytesToReceive;
                socketError = SocketError.Success;
                return true;
            }

            // Begin new fragment
            if (!this.ReceiveAtLeast(TcpHeaderLength, out socketError))
            {
                return false;
            }

            this.ReadFragmentLength();
            socketError = SocketError.Success;
            return true;
        }

        private void ReadFragmentLength()
        {
            int fragmentLength = Utilities.ToInt32BigEndian(this.buffer.AsSpan(this.readIndex, TcpHeaderLength));
            if (fragmentLength < 0)
            {
                this.lastFragment = true;
                fragmentLength &= 0x0fffffff;
            }

            if (fragmentLength % 4 != 0)
            {
                throw new RpcException("This ain't an XDR stream.");
            }

            this.readIndex += TcpHeaderLength;
            this.unreceivedBytesForCurrentFragment = (this.readIndex + fragmentLength) - this.writeIndex;
            this.endIndexOfCurrentFragment = this.writeIndex + this.unreceivedBytesForCurrentFragment;
        }

        private bool ReceiveAtLeast(int length, out SocketError socketError)
        {
            int writeIndexGoal = this.writeIndex + length;
            while (this.writeIndex < writeIndexGoal)
            {
                int receivedLength = this.socket.Receive(this.buffer, this.writeIndex, this.buffer.Length - this.writeIndex, SocketFlags.None, out socketError);
                if (socketError != SocketError.Success)
                {
                    return false;
                }

                if (receivedLength == 0)
                {
                    socketError = SocketError.NotConnected;
                    return false;
                }

                this.writeIndex += receivedLength;
            }

            socketError = SocketError.Success;
            return true;
        }
    }
}
