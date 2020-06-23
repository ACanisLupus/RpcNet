namespace RpcNet.Internal
{
    using System;
    using System.Net.Sockets;

    public class RpcTcpConnection : IDisposable
    {
        private readonly TcpClient tcpClient;
        private readonly TcpReader reader;
        private readonly TcpWriter writer;
        private readonly ReceivedCall receivedCall;

        public RpcTcpConnection(
            TcpClient tcpClient,
            int program,
            int[] versions,
            Action<ReceivedCall> receivedCallDispatcher)
        {
            this.tcpClient = tcpClient;
            this.reader = new TcpReader(tcpClient.Client);
            //this.reader.Completed += this.ReadingCompleted;
            this.writer = new TcpWriter(tcpClient.Client);
            //this.writer.Completed += this.WritingCompleted;

            this.receivedCall = new ReceivedCall(
                program,
                versions,
                this.reader,
                this.writer,
                receivedCallDispatcher);

            //this.reader.BeginReadingAsync();
        }

        public void Dispose() => this.tcpClient.Dispose();
    }
}
