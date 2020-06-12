namespace RpcNet.Internal
{
    using System;
    using System.Net.Sockets;

    // Public for tests
    public class TcpBufferReader : INetworkReader
    {
        private const int TcpHeaderLength = 4;

        private readonly Socket socket;
        private readonly byte[] buffer;

        private int readIndex;
        private int writeIndex;
        private bool lastPacket;
        private int headerIndex = 0;
        private int bodyIndex = 0;
        private PacketState packetState = PacketState.Header;

        public TcpBufferReader(Socket socket) : this(socket, 65536)
        {
        }

        public TcpBufferReader(Socket socket, int bufferSize)
        {
            if (bufferSize < TcpHeaderLength + sizeof(int) || bufferSize % 4 != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }

            this.socket = socket;
            this.buffer = new byte[bufferSize];
        }

        public bool BeginReading(out SocketError socketError)
        {
            this.readIndex = 0;
            this.writeIndex = 0;
            this.lastPacket = false;

            this.packetState = PacketState.Header;
            this.headerIndex = 0;
            this.bodyIndex = 0;

            return this.FillBuffer(out socketError);
        }

        public void EndReading()
        {
        }

        public ReadOnlySpan<byte> Read(int length)
        {
            if (!this.FillBuffer(out SocketError socketError))
            {
                throw new RpcException($"Could not receive from TCP stream. Socket error code: {socketError}.");
            }

            int endIndex = Math.Min(this.headerIndex, this.buffer.Length);
            endIndex = Math.Min(endIndex, this.writeIndex);
            int availableBytes = endIndex - this.readIndex;
            int bytesToRead = Math.Min(availableBytes, length);

            Span<byte> span = this.buffer.AsSpan(this.readIndex, bytesToRead);
            this.readIndex += bytesToRead;
            return span;
        }

        private bool FillBuffer(out SocketError socketError)
        {
            bool readFromNetwork = false;
            while (true)
            {
                if (this.packetState == PacketState.Complete)
                {
                    socketError = SocketError.Success;
                    return true;
                }

                if (this.packetState == PacketState.Header)
                {
                    this.ReadHeader(ref readFromNetwork);
                }

                if (this.packetState == PacketState.Body)
                {
                    if (this.ReadBody(ref readFromNetwork))
                    {
                        socketError = SocketError.Success;
                        return true;
                    }
                }

                if (readFromNetwork)
                {
                    if (!this.ReadFromNetwork(out socketError, ref readFromNetwork))
                    {
                        return false;
                    }
                }

                this.ShiftData();
            }
        }

        private void ShiftData()
        {
            if (this.readIndex == this.writeIndex && this.writeIndex > 0)
            {
                this.bodyIndex -= this.writeIndex;
                this.headerIndex -= this.writeIndex;
                this.writeIndex = 0;
                this.readIndex = 0;
            }
        }

        private bool ReadBody(ref bool readFromNetwork)
        {
            if (this.writeIndex == this.headerIndex && this.lastPacket)
            {
                this.packetState = PacketState.Complete;
                return true;
            }

            if (this.readIndex < this.headerIndex)
            {
                if (this.writeIndex < this.headerIndex && this.writeIndex < this.buffer.Length)
                {
                    readFromNetwork = true;
                }
                else if (this.readIndex < this.buffer.Length)
                {
                    return true;
                }
            }
            else
            {
                this.packetState = PacketState.Header;
            }

            return false;
        }

        private bool ReadFromNetwork(out SocketError socketError, ref bool readFromNetwork)
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
            readFromNetwork = false;
            return true;
        }

        private void ReadHeader(ref bool readFromNetwork)
        {
            if (this.writeIndex >= this.headerIndex + TcpHeaderLength)
            {
                int packetLength = Utilities.ToInt32BigEndian(this.buffer.AsSpan(this.headerIndex, TcpHeaderLength));
                if (packetLength < 0)
                {
                    this.lastPacket = true;
                    packetLength &= 0x0fffffff;
                }

                if (packetLength % 4 != 0 || packetLength == 0)
                {
                    throw new RpcException("This ain't an XDR stream.");
                }

                this.packetState = PacketState.Body;
                this.bodyIndex = this.headerIndex + TcpHeaderLength;
                this.headerIndex = this.bodyIndex + packetLength;
                this.readIndex = this.bodyIndex;
            }
            else
            {
                readFromNetwork = true;
            }
        }

        private enum PacketState
        {
            Header,
            Body,
            Complete
        }
    }
}
