namespace RpcNet.Internal
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using RpcNet.PortMapper;

    // Public for tests
    public class RpcUdpServer : IDisposable
    {
        private readonly ILogger logger;
        private readonly int port;
        private readonly UdpReader reader;
        private readonly ReceivedRpcCall receivedCall;
        private readonly Socket server;
        private readonly UdpWriter writer;

        private bool isDisposed;
        private Thread receivingThread;
        private volatile bool stopReceiving;

        public RpcUdpServer(
            IPAddress ipAddress,
            int program,
            int[] versions,
            Action<ReceivedRpcCall> receivedCallDispatcher,
            ServerSettings serverSettings = default)
        {
            this.port = serverSettings?.Port ?? 0;
            this.server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.server.Bind(new IPEndPoint(ipAddress, this.port));

            this.reader = new UdpReader(this.server);
            this.writer = new UdpWriter(this.server);

            this.receivedCall = new ReceivedRpcCall(
                program,
                versions,
                this.reader,
                this.writer,
                receivedCallDispatcher);

            this.logger = serverSettings?.Logger;

            if (this.port == 0)
            {
                this.port = ((IPEndPoint)this.server.LocalEndPoint).Port;
                var clientSettings = new PortMapperClientSettings
                    { Port = serverSettings?.PortMapperPort ?? PortMapperConstants.PortMapperPort };
                foreach (int version in versions)
                {
                    PortMapperUtilities.UnsetAndSetPort(Protocol.Udp, this.port, program, version, clientSettings);
                }
            }

            this.logger?.Info(
                $"{Utilities.ConvertToString(Protocol.Udp)} Server listening on {this.server.LocalEndPoint}...");
        }

        public void Start()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(RpcUdpServer));
            }

            if (this.receivingThread != null)
            {
                return;
            }

            this.receivingThread = new Thread(this.HandlingUdpCalls)
                { IsBackground = true, Name = $"RpcNet UDP {this.port}" };
            this.receivingThread.Start();
        }

        public void Dispose()
        {
            this.stopReceiving = true;
            try
            {
                // Necessary for Linux. Dispose doesn't abort synchronous calls
                this.server.Shutdown(SocketShutdown.Both);
            }
            catch
            {
                // Ignored
            }

            this.server.Dispose();
            Interlocked.Exchange(ref this.receivingThread, null)?.Join();
            this.isDisposed = true;
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
                        this.logger?.Trace($"Could not read UDP data. Socket error: {result.SocketError}.");
                        continue;
                    }

                    if (result.IsDisconnected)
                    {
                        // Should only happen on dispose
                        continue;
                    }

                    this.writer.BeginWriting();
                    this.receivedCall.HandleCall(new Caller(result.RemoteIpEndPoint, Protocol.Udp));
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
