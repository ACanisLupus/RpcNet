namespace RpcNet.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    public class RpcTcpServer : IDisposable
    {
        private readonly List<RpcTcpConnection> connections = new List<RpcTcpConnection>();
        private readonly ILogger logger;
        private readonly int program;
        private readonly Action<ReceivedCall> receivedCallDispatcher;
        private readonly TcpListener server;
        private readonly int[] versions;

        private volatile bool stopAccepting;

        public RpcTcpServer(
            IPAddress ipAddress,
            int port,
            int program,
            int[] versions,
            Action<ReceivedCall> receivedCallDispatcher,
            ILogger logger)
        {
            this.program = program;
            this.versions = versions;
            this.receivedCallDispatcher = receivedCallDispatcher;
            this.logger = logger;
            this.server = new TcpListener(ipAddress, port);

            try
            {
                this.server.Start();
            }
            catch (SocketException e)
            {
                throw new RpcException($"Could not start TCP listener. Socket error code: {e.SocketErrorCode}.");
            }

            if (port == 0)
            {
                port = ((IPEndPoint)this.server.Server.LocalEndPoint).Port;
                foreach (int version in versions)
                {
                    PortMapperUtilities.UnsetAndSetPort(ProtocolKind.Tcp, port, program, version);
                }
            }

            logger?.Trace($"TCP Server listening on {this.server.Server.LocalEndPoint}...");
        }

        public void Start()
        {
            Task.Run(this.AcceptingAsync);
        }

        public void Dispose()
        {
            this.stopAccepting = true;

            try
            {
                this.server.Stop();
            }
            catch (SocketException e)
            {
                this.logger?.Error($"Could not stop TCP listener. Socket error code: {e.SocketErrorCode}.");
            }

            lock (this.connections)
            {
                foreach (RpcTcpConnection connection in this.connections)
                {
                    connection.Dispose();
                }
            }
        }

        private async Task AcceptingAsync()
        {
            while (!this.stopAccepting)
            {
                try
                {
                    TcpClient tcpClient = await this.server.AcceptTcpClientAsync();
                    lock (this.connections)
                    {
                        this.connections.Add(
                            new RpcTcpConnection(
                                tcpClient,
                                this.program,
                                this.versions,
                                this.receivedCallDispatcher,
                                this.logger));

                        for (int i = this.connections.Count - 1; i >= 0; i--)
                        {
                            RpcTcpConnection connection = this.connections[i];
                            if (connection.IsFinished)
                            {
                                connection.Dispose();
                                this.connections.RemoveAt(i);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    this.logger?.Error($"The following error occurred while accepting TCP clients: {e}");
                }
            }
        }
    }
}
