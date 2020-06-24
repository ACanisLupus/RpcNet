namespace RpcNet.Internal
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public class RpcTcpConnection : IDisposable
    {
        private readonly TcpClient tcpClient;
        private readonly IPEndPoint remoteIpEndPoint;
        private readonly TcpReader reader;
        private readonly TcpWriter writer;
        private readonly ReceivedCall receivedCall;
        private readonly Thread receivingThread;
        private readonly ILogger logger;

        private volatile bool stopReceiving;

        public RpcTcpConnection(
            TcpClient tcpClient,
            int program,
            int[] versions,
            Action<ReceivedCall> receivedCallDispatcher,
            ILogger logger)
        {
            this.tcpClient = tcpClient;
            this.remoteIpEndPoint = (IPEndPoint)tcpClient.Client.RemoteEndPoint;
            this.reader = new TcpReader(tcpClient.Client, logger);
            this.writer = new TcpWriter(tcpClient.Client);
            this.logger = logger;

            this.receivedCall = new ReceivedCall(
                program,
                versions,
                this.reader,
                this.writer,
                receivedCallDispatcher);

            this.receivingThread = new Thread(this.Receiving) { IsBackground = true };
            this.receivingThread.Start();
        }

        public bool IsFinished { get; private set; }

        public void Dispose()
        {
            this.stopReceiving = true;
            this.tcpClient.Dispose();
            this.receivingThread.Join();
        }

        private void Receiving()
        {
            try
            {
                while (!this.stopReceiving)
                {
                    NetworkResult result = this.reader.BeginReading();
                    if (result.SocketError != SocketError.Success)
                    {
                        this.logger?.Trace($"Could not read data from {this.remoteIpEndPoint}. Socket error: {result.SocketError}.");
                        this.IsFinished = true;
                        return;
                    }

                    this.logger?.Trace($"TCP call received from {this.remoteIpEndPoint}.");
                    this.writer.BeginWriting();
                    this.receivedCall.HandleCall(this.remoteIpEndPoint);
                    this.reader.EndReading();
                    result = this.writer.EndWriting();
                    if (result.SocketError != SocketError.Success)
                    {
                        this.logger?.Trace($"Could not write data to {this.remoteIpEndPoint}. Socket error: {result.SocketError}.");
                        this.IsFinished = true;
                        return;
                    }
                }
            }
            catch (Exception exception)
            {
                this.logger?.Error($"Unexpected exception during call: {exception}");
            }
        }
    }
}
