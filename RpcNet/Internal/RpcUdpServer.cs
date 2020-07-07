namespace RpcNet.Internal
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;

    public class RpcUdpServer : IDisposable
    {
        private readonly UdpReader reader;
        private readonly ReceivedCall receivedCall;
        private readonly UdpWriter writer;
        private readonly ILogger logger;

        private volatile bool stopReceiving;

        public RpcUdpServer(
            IPAddress ipAddress,
            int port,
            int program,
            int[] versions,
            Action<ReceivedCall> receivedCallDispatcher,
            ILogger logger)
        {
            var server = new UdpClient(new IPEndPoint(ipAddress, port));

            this.reader = new UdpReader(server.Client, logger);
            this.reader.Completed += this.ReadingCompleted;
            this.writer = new UdpWriter(server.Client, logger);
            this.writer.Completed += this.WritingCompleted;

            this.receivedCall = new ReceivedCall(program, versions, this.reader, this.writer, receivedCallDispatcher);

            this.logger = logger;

            if (port == 0)
            {
                port = ((IPEndPoint)server.Client.LocalEndPoint).Port;
                PortMapperUtilities.UnsetAndSetPort(ProtocolKind.Udp, port, program, versions.Last());
            }

            this.logger?.Trace($"UDP Server listening on {server.Client.LocalEndPoint}...");

            this.reader.BeginReadingAsync();
        }

        public void Dispose()
        {
            this.stopReceiving = true;
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
                string errorMessage = $"Unexpected exception during UDP call from {udpResult.RemoteIpEndPoint}: {rpcException}";
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
