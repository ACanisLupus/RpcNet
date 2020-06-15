namespace RpcNet.Internal
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    // Public for tests
    public class RpcUdpServer : IDisposable
    {
        private readonly UdpClient server;
        private readonly UdpServiceReader udpBufferReader;
        private readonly UdpServiceWriter udpBufferWriter;
        private readonly ReceivedCall receivedCall;

        private volatile bool stopReceiving;

        public RpcUdpServer(IPAddress ipAddress, int port, int program, int[] versions, Action<ReceivedCall> receivedCallDispatcher)
        {
            this.server = new UdpClient(new IPEndPoint(ipAddress, port));

            this.udpBufferReader = new UdpServiceReader(this.server.Client);
            this.udpBufferReader.Completed += this.ReadingCompleted;
            this.udpBufferWriter = new UdpServiceWriter(this.server.Client);
            this.udpBufferWriter.Completed += this.WritingCompleted;

            this.receivedCall = new ReceivedCall(
                program,
                versions,
                this.udpBufferReader,
                this.udpBufferWriter,
                receivedCallDispatcher);
        }

        public void Dispose() => this.Stop();
        public void Start() => this.udpBufferReader.BeginReading();

        public void Stop()
        {
            this.stopReceiving = true;
            this.udpBufferReader.Dispose();
            this.udpBufferWriter.Dispose();
        }

        private void ReadingCompleted(UdpResult udpResult)
        {
            if (this.stopReceiving)
            {
                return;
            }

            try
            {
                this.udpBufferWriter.BeginWriting();
                this.receivedCall.HandleCall(udpResult.IpEndPoint);
                this.udpBufferWriter.EndWriting(udpResult.IpEndPoint);
            }
            catch (RpcException rpcException)
            {
                Console.WriteLine($"Could not handle call. {rpcException}");
            }
        }

        private void WritingCompleted(UdpResult udpResult)
        {
            if (this.stopReceiving)
            {
                return;
            }

            this.Start();
        }
    }
}
