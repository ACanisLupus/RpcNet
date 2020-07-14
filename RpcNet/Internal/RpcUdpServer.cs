namespace RpcNet.Internal
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public class RpcUdpServer : IDisposable
    {
        private readonly ILogger logger;
        private readonly UdpReader reader;
        private readonly ReceivedCall receivedCall;
        private readonly UdpClient server;
        private readonly UdpWriter writer;
        private readonly Thread udpThread;

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
            this.writer = new UdpWriter(this.server, logger);

            this.receivedCall = new ReceivedCall(program, versions, this.reader, this.writer, receivedCallDispatcher);

            this.logger = logger;

            if (port == 0)
            {
                port = ((IPEndPoint)this.server.Client.LocalEndPoint).Port;
                PortMapperUtilities.UnsetAndSetPort(ProtocolKind.Udp, port, program, versions.Last());
            }

            this.logger?.Trace($"UDP Server listening on {this.server.Client.LocalEndPoint}...");

            this.udpThread = new Thread(this.HandlingUdpCalls) { IsBackground = true };
            this.udpThread.Start();
        }

        public void Dispose()
        {
            this.stopReceiving = true;
            this.server.Dispose();
            this.reader.Dispose();
            this.writer.Dispose();
            this.udpThread.Join();
        }

        private void HandlingUdpCalls()
        {
            try
            {
                while (!this.stopReceiving)
                {
                    NetworkReadResult udpResult = default;
                    try
                    {
                        udpResult = this.reader.BeginReading();
                        if (udpResult.HasError)
                        {
                            continue;
                        }

                        this.writer.BeginWriting();
                        this.receivedCall.HandleCall(udpResult.RemoteIpEndPoint);
                        this.reader.EndReading();
                        this.writer.EndWriting(udpResult.RemoteIpEndPoint);

                    }
                    catch (RpcException rpcException)
                    {
                        string errorMessage =
                            $"Unexpected exception during UDP call from {udpResult.RemoteIpEndPoint}: {rpcException}";
                        this.logger?.Error(errorMessage);
                    }
                }
            }
            catch (Exception exception)
            {
                string errorMessage =
                    $"Unexpected exception in UDP thread: {exception}";
                this.logger?.Error(errorMessage);
            }
        }
    }
}
