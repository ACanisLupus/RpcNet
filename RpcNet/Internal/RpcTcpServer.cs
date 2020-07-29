namespace RpcNet.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    // Public for tests
    public class RpcTcpServer : IDisposable
    {
        private readonly List<RpcTcpConnection> connections = new List<RpcTcpConnection>();
        private readonly IPAddress ipAddress;
        private readonly ILogger logger;
        private readonly int port;
        private readonly int program;
        private readonly Action<ReceivedRpcCall> receivedCallDispatcher;
        private readonly Socket server;
        private readonly int[] versions;

        private Thread acceptingThread;

        private volatile bool stopAccepting;

        public RpcTcpServer(
            IPAddress ipAddress,
            int port,
            int program,
            int[] versions,
            Action<ReceivedRpcCall> receivedCallDispatcher,
            ILogger logger)
        {
            this.program = program;
            this.versions = versions;
            this.receivedCallDispatcher = receivedCallDispatcher;
            this.logger = logger;
            this.ipAddress = ipAddress;
            this.port = port;
            this.server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Start()
        {
            if (this.acceptingThread != null)
            {
                return;
            }

            try
            {
                this.server.Bind(new IPEndPoint(this.ipAddress, this.port));
                this.server.Listen(int.MaxValue);
            }
            catch (SocketException e)
            {
                throw new RpcException($"Could not start TCP listener. Socket error code: {e.SocketErrorCode}.");
            }

            if (this.port == 0)
            {
                int realPort = ((IPEndPoint)this.server.LocalEndPoint).Port;
                lock (this.connections)
                {
                    foreach (int version in this.versions)
                    {
                        PortMapperUtilities.UnsetAndSetPort(ProtocolKind.Tcp, realPort, this.program, version);
                    }
                }
            }

            this.logger?.Trace(
                $"{Utilities.ConvertToString(Protocol.Tcp)} Server listening on {this.server.LocalEndPoint}...");

            this.acceptingThread = new Thread(this.Accepting);
            this.acceptingThread.Start();
        }

        public void Dispose()
        {
            this.stopAccepting = true;

            this.server.Dispose();

            lock (this.connections)
            {
                foreach (RpcTcpConnection connection in this.connections)
                {
                    connection.Dispose();
                }

                this.connections.Clear();
            }

            Interlocked.Exchange(ref this.acceptingThread, null)?.Join();
        }

        private void Accepting()
        {
            while (!this.stopAccepting)
            {
                try
                {
                    Socket tcpClient = this.server.Accept();
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
                catch (SocketException e)
                {
                    this.logger?.Error($"Could not accept TCP client. Socket error code: {e.SocketErrorCode}");
                }
                catch (Exception e)
                {
                    this.logger?.Error($"The following error occurred while accepting TCP clients: {e}");
                }
            }
        }
    }
}
