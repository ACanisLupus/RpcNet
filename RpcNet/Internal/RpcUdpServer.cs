namespace RpcNet.Internal
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    public class RpcUdpServer : IDisposable
    {
        private readonly UdpClient server;
        private readonly UdpReader reader;
        private readonly UdpWriter writer;
        private readonly ReceivedCall receivedCall;

        private volatile bool stopReceiving;

        public RpcUdpServer(IPAddress ipAddress, int port, int program, int[] versions, Action<ReceivedCall> receivedCallDispatcher)
        {
            this.server = new UdpClient(new IPEndPoint(ipAddress, port));

            this.reader = new UdpReader(this.server.Client);
            this.reader.Completed += this.ReadingCompleted;
            this.writer = new UdpWriter(this.server.Client);
            this.writer.Completed += this.WritingCompleted;

            this.receivedCall = new ReceivedCall(
                program,
                versions,
                this.reader,
                this.writer,
                receivedCallDispatcher);

            this.reader.BeginReadingAsync();
        }

        public void Dispose()
        {
            this.stopReceiving = true;
            this.reader.Dispose();
            this.writer.Dispose();
        }

        private void ReadingCompleted(NetworkResult udpResult)
        {
            if (this.stopReceiving)
            {
                return;
            }

            try
            {
                this.writer.BeginWriting();
                this.receivedCall.HandleCall(udpResult.IpEndPoint);
                this.reader.EndReading(); // TODO: This is not good. Possible solution: RpcTcpServer works async as well and ReceivedCall takes care of this
                this.writer.EndWritingAsync(udpResult.IpEndPoint);
            }
            catch (RpcException rpcException)
            {
                Console.WriteLine($"Could not handle call. {rpcException}");
            }
        }

        private void WritingCompleted(NetworkResult udpResult)
        {
            if (this.stopReceiving)
            {
                return;
            }

            this.reader.BeginReadingAsync();
        }
    }
}
