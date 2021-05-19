namespace RpcNet.Internal
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    // Public for tests
    public class TcpWriter : INetworkWriter
    {
        private const int TcpHeaderLength = 4;

        private readonly byte[] buffer;
        private readonly ILogger logger;

        private Socket tcpClient;
        private int writeIndex;

        public TcpWriter(Socket tcpClient, ILogger logger = default) : this(tcpClient, 65536, logger)
        {
        }

        public TcpWriter(Socket tcpClient, int bufferSize, ILogger logger = default)
        {
            if ((bufferSize < (TcpHeaderLength + sizeof(int))) || ((bufferSize % 4) != 0))
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }

            this.logger = logger;

            this.Reset(tcpClient);
            this.buffer = new byte[bufferSize];
        }

        public void Reset(Socket tcpClient) => this.tcpClient = tcpClient;

        public void BeginWriting() => this.writeIndex = TcpHeaderLength;

        public NetworkWriteResult EndWriting(IPEndPoint remoteEndPoint) => this.FlushPacket(true);

        public Span<byte> Reserve(int length)
        {
            int maxLength = this.buffer.Length - this.writeIndex;

            // Integers (4 bytes) and padding bytes (> 1 and < 4 bytes) must not be sent fragmented
            if ((maxLength < length) && (maxLength < sizeof(int)))
            {
                this.FlushPacket(false);
                maxLength = this.buffer.Length - this.writeIndex;
            }

            int reservedLength = Math.Min(length, maxLength);

            Span<byte> span = this.buffer.AsSpan(this.writeIndex, reservedLength);
            this.writeIndex += reservedLength;
            return span;
        }

        private NetworkWriteResult FlushPacket(bool lastPacket)
        {
            int length = this.writeIndex - TcpHeaderLength;

            // Last fragment sets first bit to 1
            int lengthToDecode = lastPacket ? length | unchecked((int)0x80000000) : length;

            Utilities.WriteBytesBigEndian(this.buffer.AsSpan(), lengthToDecode);

            SocketError socketError;
            try
            {
                this.tcpClient.Send(
                    this.buffer,
                    0,
                    length + TcpHeaderLength,
                    SocketFlags.None,
                    out socketError);
            }
            catch (SocketException exception)
            {
                return new NetworkWriteResult(exception.SocketErrorCode);
            }
            catch (Exception exception)
            {
                this.logger?.Error(
                    $"Unexpected error while sending TCP data to {this.tcpClient?.RemoteEndPoint}: {exception}");
                return new NetworkWriteResult(SocketError.SocketError);
            }

            if (socketError == SocketError.Success)
            {
                this.BeginWriting();
            }

            return new NetworkWriteResult(socketError);
        }
    }
}
