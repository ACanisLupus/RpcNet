namespace RpcNet.Internal
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    public class TcpReader : INetworkReader
    {
        private const int TcpHeaderLength = 4;

        private readonly Socket socket;
        private readonly byte[] buffer;
        private readonly IPEndPoint remoteIpEndPoint;

        private int readIndex;
        private int writeIndex;
        private bool lastPacket;
        private int headerIndex = 0;
        private int bodyIndex = 0;
        private PacketState packetState = PacketState.Header;

        public TcpReader(Socket socket) : this(socket, 65536)
        {
        }

        public TcpReader(Socket socket, int bufferSize)
        {
            if (bufferSize < TcpHeaderLength + sizeof(int) || bufferSize % 4 != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }

            this.socket = socket;
            this.remoteIpEndPoint = (IPEndPoint)socket.RemoteEndPoint;
            this.buffer = new byte[bufferSize];
        }

        public NetworkResult BeginReading()
        {
            this.readIndex = 0;
            this.writeIndex = 0;
            this.lastPacket = false;

            this.packetState = PacketState.Header;
            this.headerIndex = 0;
            this.bodyIndex = 0;

            SocketError socketError = this.FillBuffer();

            return new NetworkResult
            {
                IpEndPoint = this.remoteIpEndPoint,
                SocketError = socketError
            };
        }

        public void EndReading()
        {
            if (this.packetState != PacketState.Complete || this.readIndex != this.writeIndex)
            {
                throw new RpcException("Not all data was read.");
            }
        }

        public ReadOnlySpan<byte> Read(int length)
        {
            SocketError socketError = this.FillBuffer();
            if (socketError != SocketError.Success)
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

        // On the first iteration, this function will read as many data from the network as available
        // On the following iterations, it depends on the yet received data:
        // - Not enough bytes for header? Read from network again
        // - Packet is not complete and there is space left in the buffer? Read from network again
        // - Packet is not complete and no space available? Return and wait for XDR read
        // - Packet is complete and XDR read is not complete? Return and wait for XDF read
        // - Packet and XDR read is complete? Read next header. Or finish if previous packet was the last packet
        private SocketError FillBuffer()
        {
            bool readFromNetwork = false;
            while (true)
            {
                if (this.packetState == PacketState.Complete)
                {
                    return SocketError.Success;
                }

                if (this.packetState == PacketState.Header)
                {
                    this.ReadHeader(ref readFromNetwork);
                }

                if (this.packetState == PacketState.Body)
                {
                    if (this.ReadBody(ref readFromNetwork))
                    {
                        return SocketError.Success;
                    }
                }

                if (readFromNetwork)
                {
                    SocketError socketError = this.ReadFromNetwork(ref readFromNetwork);
                    if (socketError != SocketError.Success)
                    {
                        return socketError;
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

        private SocketError ReadFromNetwork(ref bool readFromNetwork)
        {
            int receivedLength = this.socket.Receive(this.buffer, this.writeIndex, this.buffer.Length - this.writeIndex, SocketFlags.None, out SocketError socketError);
            if (socketError != SocketError.Success)
            {
                return socketError;
            }

            if (receivedLength == 0)
            {
                return SocketError.NotConnected;
            }

            this.writeIndex += receivedLength;
            readFromNetwork = false;
            return socketError;
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
