namespace RpcNet.Internal
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;

    public class RpcUdpServer : IDisposable
    {
        private readonly ILogger logger;
        private readonly UdpReader reader;
        private readonly ReceivedCall receivedCall;
        private readonly UdpClient server;
        private readonly UdpWriter writer;

        private volatile bool stopReceiving;

        public RpcUdpServer(
            IPAddress ipAddress,
            int port,
            int program,
            int[] versions,
            Action<ReceivedCall> receivedCallDispatcher,
            ILogger logger)
        {
            this.server = new UdpClient(new IPEndPoint(ipAddress, port));

            this.reader = new UdpReader(this.server, logger);
            this.reader.Completed += this.ReadingCompleted;
            this.writer = new UdpWriter(this.server, logger);
            this.writer.Completed += this.WritingCompleted;

            this.receivedCall = new ReceivedCall(program, versions, this.reader, this.writer, receivedCallDispatcher);

            this.logger = logger;

            if (port == 0)
            {
                port = ((IPEndPoint)this.server.Client.LocalEndPoint).Port;
                PortMapperUtilities.UnsetAndSetPort(ProtocolKind.Udp, port, program, versions.Last());
            }

            this.logger?.Trace($"UDP Server listening on {this.server.Client.LocalEndPoint}...");

            this.reader.BeginReadingAsync();
        }

        public void Dispose()
        {
            this.stopReceiving = true;
            this.server.Dispose();
            this.reader.Dispose();
            this.writer.Dispose();
        }

        private void ReadingCompleted(NetworkReadResult udpResult)
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
                string errorMessage =
                    $"Unexpected exception during UDP call from {udpResult.RemoteIpEndPoint}: {rpcException}";
                this.logger?.Error(errorMessage);
            }
        }

        private void WritingCompleted(NetworkWriteResult udpResult)
        {
            if (this.stopReceiving)
            {
                return;
            }

            this.reader.BeginReadingAsync();
        }
    }
}
