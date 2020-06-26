namespace RpcNet.Internal
{
    using System;
    using System.Net.Sockets;

    public class TcpReader : INetworkReader
    {
        private const int TcpHeaderLength = 4;

        private readonly byte[] buffer;
        private readonly ILogger logger;

        private int bodyIndex;
        private int headerIndex;
        private bool lastPacket;
        private PacketState packetState = PacketState.Header;
        private int readIndex;
        private Socket socket;
        private int writeIndex;

        public TcpReader(Socket socket, ILogger logger) : this(socket, 65536, logger)
        {
        }

        public TcpReader(Socket socket, int bufferSize, ILogger logger)
        {
            if (bufferSize < TcpHeaderLength + sizeof(int) || bufferSize % 4 != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }

            this.Reset(socket);
            this.buffer = new byte[bufferSize];
            this.logger = logger;
        }

        public void Reset(Socket socket) => this.socket = socket;

        public NetworkReadResult BeginReading()
        {
            this.readIndex = 0;
            this.writeIndex = 0;
            this.lastPacket = false;
            this.headerIndex = 0;
            this.bodyIndex = 0;
            this.packetState = PacketState.Header;

            return this.FillBuffer();
        }

        public void EndReading()
        {
            if (this.packetState != PacketState.Complete || this.readIndex != this.writeIndex)
            {
                const string ErrorMessage = "Not all data was read.";
                this.logger?.Error(ErrorMessage);
                throw new RpcException(ErrorMessage);
            }
        }

        public ReadOnlySpan<byte> Read(int length)
        {
            NetworkReadResult networkReadResult = this.FillBuffer();
            if (networkReadResult.HasError)
            {
                string errorMessage = $"Could not receive from TCP stream. Socket error code: {networkReadResult.SocketError}.";
                this.logger?.Error(errorMessage);
                throw new RpcException(errorMessage);
            }

            if (networkReadResult.IsDisconnected)
            {
                const string ErrorMessage = "Could not receive from TCP stream. Remote end point disconnected.";
                this.logger?.Error(ErrorMessage);
                throw new RpcException(ErrorMessage);
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
        private NetworkReadResult FillBuffer()
        {
            bool readFromNetwork = false;
            while (true)
            {
                if (this.packetState == PacketState.Complete)
                {
                    return NetworkReadResult.CreateSuccess();
                }

                if (this.packetState == PacketState.Header)
                {
                    this.ReadHeader(ref readFromNetwork);
                }

                if (this.packetState == PacketState.Body)
                {
                    if (this.ReadBody(ref readFromNetwork))
                    {
                        return NetworkReadResult.CreateSuccess();
                    }
                }

                if (readFromNetwork)
                {
                    NetworkReadResult networkReadResult = this.ReadFromNetwork(ref readFromNetwork);
                    if (networkReadResult.HasError || networkReadResult.IsDisconnected)
                    {
                        return networkReadResult;
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

        private NetworkReadResult ReadFromNetwork(ref bool readFromNetwork)
        {
            int receivedLength = this.socket.Receive(
                this.buffer,
                this.writeIndex,
                this.buffer.Length - this.writeIndex,
                SocketFlags.None,
                out SocketError socketError);
            if (socketError != SocketError.Success)
            {
                return NetworkReadResult.CreateError(socketError);
            }

            if (receivedLength == 0)
            {
                return NetworkReadResult.CreateDisconnected();
            }

            this.writeIndex += receivedLength;
            readFromNetwork = false;
            return NetworkReadResult.CreateSuccess();
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
                    const string ErrorMessage = "This is not an XDR stream.";
                    this.logger?.Error(ErrorMessage);
                    throw new RpcException(ErrorMessage);
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
