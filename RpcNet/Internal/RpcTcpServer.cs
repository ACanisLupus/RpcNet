namespace RpcNet.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using RpcNet.PortMapper;

    // Public for tests
    public class RpcTcpServer : IDisposable
    {
        private readonly List<RpcTcpConnection> connections = new List<RpcTcpConnection>();
        private readonly IPAddress ipAddress;
        private readonly ILogger logger;
        private readonly int portMapperPort;
        private readonly int program;
        private readonly Action<ReceivedRpcCall> receivedCallDispatcher;
        private readonly Socket server;
        private readonly int[] versions;

        private Thread acceptingThread;
        private bool isDisposed;
        private int port;
        private volatile bool stopAccepting;

        public RpcTcpServer(
            IPAddress ipAddress,
            int program,
            int[] versions,
            Action<ReceivedRpcCall> receivedCallDispatcher,
            ServerSettings serverSettings = default)
        {
            this.program = program;
            this.versions = versions;
            this.receivedCallDispatcher = receivedCallDispatcher;
            this.logger = serverSettings?.Logger;
            this.ipAddress = ipAddress;
            this.port = serverSettings?.Port ?? 0;
            this.portMapperPort = serverSettings?.PortMapperPort ?? PortMapperConstants.PortMapperPort;
            this.server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Start()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(RpcUdpServer));
            }

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
                throw new RpcException($"Could not start TCP listener. Socket error: {e.SocketErrorCode}.");
            }

            if (this.port == 0)
            {
                this.port = ((IPEndPoint)this.server.LocalEndPoint).Port;
                lock (this.connections)
                {
                    var clientSettings = new PortMapperClientSettings { Port = this.portMapperPort };
                    foreach (int version in this.versions)
                    {
                        PortMapperUtilities.UnsetAndSetPort(
                            Protocol.Tcp,
                            this.port,
                            this.program,
                            version,
                            clientSettings);
                    }
                }
            }

            this.logger?.Info(
                $"{Utilities.ConvertToString(Protocol.Tcp)} Server listening on {this.server.LocalEndPoint}...");

            this.acceptingThread = new Thread(this.Accepting) { Name = $"RpcNet TCP Accept {this.port}" };
            this.acceptingThread.Start();
        }

        public void Dispose()
        {
            this.stopAccepting = true;
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

            lock (this.connections)
            {
                foreach (RpcTcpConnection connection in this.connections)
                {
                    connection.Dispose();
                }

                this.connections.Clear();
            }

            Interlocked.Exchange(ref this.acceptingThread, null)?.Join();
            this.isDisposed = true;
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
                    this.logger?.Error($"Could not accept TCP client. Socket error: {e.SocketErrorCode}");
                }
                catch (Exception e)
                {
                    this.logger?.Error($"The following error occurred while accepting TCP clients: {e}");
                }
            }
        }
    }
}
