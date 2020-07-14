namespace RpcNet.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public class RpcTcpServer : IDisposable
    {
        private readonly List<RpcTcpConnection> connections = new List<RpcTcpConnection>();
        private readonly ILogger logger;
        private readonly int port;
        private readonly int program;
        private readonly Action<ReceivedCall> receivedCallDispatcher;
        private readonly TcpListener server;
        private readonly int[] versions;
        private Thread acceptingThread;

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
            this.port = port;
            this.server = new TcpListener(ipAddress, port);
        }

        public void Start()
        {
            if (this.acceptingThread != null)
            {
                return;
            }

            try
            {
                this.server.Start();
            }
            catch (SocketException e)
            {
                throw new RpcException($"Could not start TCP listener. Socket error code: {e.SocketErrorCode}.");
            }

            if (this.port == 0)
            {
                int realPort = ((IPEndPoint)this.server.Server.LocalEndPoint).Port;
                foreach (int version in this.versions)
                {
                    PortMapperUtilities.UnsetAndSetPort(ProtocolKind.Tcp, realPort, this.program, version);
                }
            }

            this.logger?.Trace($"TCP Server listening on {this.server.Server.LocalEndPoint}...");

            this.acceptingThread = new Thread(this.Accepting);
            this.acceptingThread.Start();
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

            this.acceptingThread?.Join();
        }

        private void Accepting()
        {
            while (!this.stopAccepting)
            {
                try
                {
                    TcpClient tcpClient = this.server.AcceptTcpClient();
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
