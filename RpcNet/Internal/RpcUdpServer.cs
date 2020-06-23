﻿namespace RpcNet.Internal
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    public class RpcUdpServer : IDisposable
    {
        private readonly UdpReader reader;
        private readonly ReceivedCall receivedCall;
        private readonly UdpWriter writer;

        private volatile bool stopReceiving;

        public RpcUdpServer(
            IPAddress ipAddress,
            int port,
            int program,
            int[] versions,
            Action<ReceivedCall> receivedCallDispatcher)
        {
            var server = new UdpClient(new IPEndPoint(ipAddress, port));

            this.reader = new UdpReader(server.Client);
            this.reader.Completed += this.ReadingCompleted;
            this.writer = new UdpWriter(server.Client);
            this.writer.Completed += this.WritingCompleted;

            this.receivedCall = new ReceivedCall(program, versions, this.reader, this.writer, receivedCallDispatcher);

            this.reader.BeginReadingAsync();
        }

        public void Dispose()
        {
            this.stopReceiving = true;
            this.reader.Dispose();
            this.writer.Dispose();
        }

        private void ReadingCompleted(NetworkResult udpResult)
        {
            if (this.stopReceiving)
            {
                return;
            }

            try
            {
                this.writer.BeginWriting();
                this.receivedCall.HandleCall(udpResult.RemoteIpEndPoint);
                this.reader.EndReading();
                this.writer.EndWritingAsync(udpResult.RemoteIpEndPoint);
            }
            catch (RpcException rpcException)
            {
                Console.WriteLine($"Could not handle call. {rpcException}");
            }
        }

        private void WritingCompleted(NetworkResult udpResult)
        {
            if (this.stopReceiving)
            {
                return;
            }

            this.reader.BeginReadingAsync();
        }
    }
}
