namespace RpcNet.Internal
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public class RpcUdpServer : IDisposable
    {
        private readonly ILogger logger;
        private readonly UdpReader reader;
        private readonly ReceivedCall receivedCall;
        private readonly Thread receivingThread;
        private readonly Socket server;
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
            this.server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.server.Bind(new IPEndPoint(ipAddress, port));

            this.reader = new UdpReader(this.server);
            this.writer = new UdpWriter(this.server);

            this.receivedCall = new ReceivedCall(program, versions, this.reader, this.writer, receivedCallDispatcher);

            this.logger = logger;

            if (port == 0)
            {
                port = ((IPEndPoint)this.server.LocalEndPoint).Port;
                foreach (int version in versions)
                {
                    PortMapperUtilities.UnsetAndSetPort(ProtocolKind.Udp, port, program, version);
                }
            }

            this.logger?.Trace($"UDP Server listening on {this.server.LocalEndPoint}...");
            this.receivingThread = new Thread(this.HandlingUdpCalls) { IsBackground = true };
        }

        public void Start()
        {
            this.receivingThread.Start();
        }

        public void Dispose()
        {
            this.stopReceiving = true;
            this.server.Dispose();
            this.receivingThread.Join();
        }

        private void HandlingUdpCalls()
        {
            while (!this.stopReceiving)
            {
                try
                {
                    NetworkReadResult result = this.reader.BeginReading();
                    if (result.HasError)
                    {
                        this.logger?.Trace(
                            $"Could not read data from {result.RemoteIpEndPoint}. " +
                            $"Socket error: {result.SocketError}.");
                        continue;
                    }
                    this.writer.BeginWriting();
                    this.receivedCall.HandleCall(result.RemoteIpEndPoint);
                    this.reader.EndReading();
                    this.writer.EndWriting(result.RemoteIpEndPoint);
                }
                catch (Exception e)
                {
                    this.logger?.Error($"The following error occurred while processing UDP call: {e}");
                }
            }
        }
    }
}
