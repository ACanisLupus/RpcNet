﻿namespace RpcNet.Internal
{
    using System;
    using System.Net.Sockets;

    public class TcpWriter : INetworkWriter
    {
        private const int TcpHeaderLength = 4;

        private readonly byte[] buffer;

        private Socket socket;
        private int writeIndex;

        public TcpWriter(Socket socket) : this(socket, 65536)
        {
        }

        public TcpWriter(Socket socket, int bufferSize)
        {
            if (bufferSize < TcpHeaderLength + sizeof(int) || bufferSize % 4 != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }

            this.Reset(socket);
            this.buffer = new byte[bufferSize];
        }

        public void Reset(Socket socket) => this.socket = socket;
        public void BeginWriting() => this.writeIndex = TcpHeaderLength;
        public NetworkWriteResult EndWriting() => this.FlushPacket(true);

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

        private NetworkWriteResult FlushPacket(bool lastPacket)
        {
            int length = this.writeIndex - TcpHeaderLength;

            // Last fragment sets first bit to 1
            int lengthToDecode = lastPacket ? length | unchecked((int)0x80000000) : length;

            Utilities.WriteBytesBigEndian(this.buffer.AsSpan(), lengthToDecode);
            this.socket.Send(this.buffer, 0, length + TcpHeaderLength, SocketFlags.None, out SocketError socketError);
            if (socketError == SocketError.Success)
            {
                this.BeginWriting();
            }

            return new NetworkWriteResult(socketError);

            // For test purposes. This simulates a slow network
            //for (int i = 0; i < length + TcpHeader; i++)
            //{
            //    byte[] tmpBuffer = new byte[1] { this.buffer[i] };
            //    this.socket.Send(tmpBuffer, 0, 1, SocketFlags.None, out SocketError socketError);
            //    if (socketError != SocketError.Success)
            //    {
            //        throw new RpcException($"Could not send packet. Socket error code: {socketError}.");
            //    }
            //    System.Threading.Thread.Sleep(1);
            //}
        }
    }
}
