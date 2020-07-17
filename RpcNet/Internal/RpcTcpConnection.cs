namespace RpcNet.Internal
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public class RpcTcpConnection : IDisposable
    {
        private readonly ILogger logger;
        private readonly TcpReader reader;
        private readonly ReceivedCall receivedCall;
        private readonly Thread receivingThread;
        private readonly IPEndPoint remoteIpEndPoint;
        private readonly Socket tcpClient;
        private readonly TcpWriter writer;

        private volatile bool stopReceiving;

        public RpcTcpConnection(
            Socket tcpClient,
            int program,
            int[] versions,
            Action<ReceivedCall> receivedCallDispatcher,
            ILogger logger)
        {
            this.tcpClient = tcpClient;
            this.remoteIpEndPoint = (IPEndPoint)tcpClient.RemoteEndPoint;
            this.reader = new TcpReader(tcpClient);
            this.writer = new TcpWriter(tcpClient);
            this.logger = logger;

            this.receivedCall = new ReceivedCall(program, versions, this.reader, this.writer, receivedCallDispatcher);

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
                    NetworkReadResult readResult = this.reader.BeginReading();
                    if (readResult.HasError)
                    {
                        this.logger?.Trace(
                            $"Could not read data from {this.remoteIpEndPoint}. " +
                            $"Socket error: {readResult.SocketError}.");
                        this.IsFinished = true;
                        return;
                    }

                    if (readResult.IsDisconnected)
                    {
                        this.logger?.Trace($"{this.remoteIpEndPoint} disconnected.");
                        this.IsFinished = true;
                        return;
                    }

                    this.writer.BeginWriting();
                    this.receivedCall.HandleCall(this.remoteIpEndPoint);
                    this.reader.EndReading();

                    NetworkWriteResult writeResult = this.writer.EndWriting();
                    if (writeResult.HasError)
                    {
                        this.logger?.Trace(
                            $"Could not write data to {this.remoteIpEndPoint}. " +
                            $"Socket error: {writeResult.SocketError}.");
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
