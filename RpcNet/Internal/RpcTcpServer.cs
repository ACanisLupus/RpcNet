namespace RpcNet.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;

    public class RpcTcpServer : IDisposable
    {
        private readonly SortedSet<RpcTcpConnection> connections = new SortedSet<RpcTcpConnection>();
        private readonly int program;
        private readonly Action<ReceivedCall> receivedCallDispatcher;
        private readonly TcpListener server;
        private readonly int[] versions;
        private readonly ILogger logger;

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
            this.server.Start();
            this.server.BeginAcceptTcpClient(this.OnAccepted, null);
        }

        public void Dispose()
        {
            this.stopAccepting = true;
            this.server.Stop();

            lock (this.connections)
            {
                foreach (RpcTcpConnection connection in this.connections)
                {
                    connection.Dispose();
                }
            }
        }

        private void OnAccepted(IAsyncResult asyncResult)
        {
            if (this.stopAccepting)
            {
                return;
            }

            TcpClient tcpClient = this.server.EndAcceptTcpClient(asyncResult);
            lock (this.connections)
            {
                this.connections.Add(
                    new RpcTcpConnection(tcpClient, this.program, this.versions, this.receivedCallDispatcher, this.logger));

                foreach (RpcTcpConnection connection in this.connections)
                {
                    if (connection.IsFinished)
                    {
                        connection.Dispose();
                    }
                }

                this.connections.RemoveWhere(match => match.IsFinished);
            }

            this.server.BeginAcceptTcpClient(this.OnAccepted, null);
        }
    }
}
