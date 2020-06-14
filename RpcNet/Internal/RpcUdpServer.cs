namespace RpcNet.Internal
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    // Public for tests
    public class RpcUdpServer : IDisposable
    {
        private readonly UdpClient server;
        private readonly Thread receiverThread;
        private readonly UdpBufferReader udpBufferReader;
        private readonly UdpBufferWriter udpBufferWriter;
        private readonly ReceivedCall receivedCall;

        private volatile bool stopReceiving;

        public RpcUdpServer(IPAddress ipAddress, int port, int program, int[] versions, Action<ReceivedCall> receivedCallDispatcher)
        {
            this.server = new UdpClient(new IPEndPoint(ipAddress, port));
            this.receiverThread = new Thread(this.DoReceiving);

            this.udpBufferReader = new UdpBufferReader(this.server.Client);
            this.udpBufferWriter = new UdpBufferWriter(this.server.Client);

            this.receivedCall = new ReceivedCall(
                program,
                versions,
                this.udpBufferReader,
                this.udpBufferWriter,
                receivedCallDispatcher);
        }

        public void Dispose() => this.Stop();
        public void Start() => this.receiverThread.Start();

        public void Stop()
        {
            this.stopReceiving = true;
            this.udpBufferReader.Dispose();
            this.udpBufferWriter.Dispose();
        }

        private void DoReceiving()
        {
            try
            {
                while (!this.stopReceiving)
                {
                    EndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Loopback, 0);
                    UdpResult result = this.udpBufferReader.BeginReading();
                    if (result.SocketError != SocketError.Success || result.BytesLength == 0)
                    {
                        continue;
                    }

                    this.udpBufferWriter.BeginWriting();
                    try
                    {
                        this.receivedCall.HandleCall(result.IpEndPoint);
                    }
                    catch (RpcException rpcException)
                    {
                        Console.WriteLine($"Could not handle call. {rpcException}");
                        continue;
                    }

                    _ = this.udpBufferWriter.EndWriting(result.IpEndPoint);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Unexpected exception in UDP receiver thread: {exception}");
            }
        }
    }
}
