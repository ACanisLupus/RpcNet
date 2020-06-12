namespace RpcNet.Internal
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    // Public for tests
    public class RpcUdpServer
    {
        private readonly UdpClient server;
        private readonly Thread receiverThread;
        private readonly XdrReader xdrReader;
        private readonly XdrWriter xdrWriter;
        private readonly UdpBufferReader udpBufferReader;
        private readonly UdpBufferWriter udpBufferWriter;
        private readonly Action<ReceivedCall> receivedCallDispatcher;

        private volatile bool stopReceiving;

        public RpcUdpServer(IPAddress ipAddress, int port, Action<ReceivedCall> receivedCallDispatcher)
        {
            this.server = new UdpClient(new IPEndPoint(ipAddress, port));
            this.receiverThread = new Thread(this.DoReceiving);

            this.udpBufferReader = new UdpBufferReader(this.server.Client);
            this.xdrReader = new XdrReader(this.udpBufferReader);

            this.udpBufferWriter = new UdpBufferWriter(this.server.Client);
            this.xdrWriter = new XdrWriter(this.udpBufferWriter);

            this.receivedCallDispatcher = receivedCallDispatcher;
        }

        public void Start()
        {
            this.receiverThread.Start();
        }

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

                    var receivedCall = new ReceivedCall(result.IpEndPoint, this.xdrReader, this.xdrWriter);

                    this.udpBufferWriter.BeginWriting();
                    try
                    {
                        this.receivedCallDispatcher(receivedCall);
                    }
                    catch (RpcException rpcException)
                    {
                        Console.WriteLine($"Could not read data from {result.IpEndPoint}. {rpcException}");
                        continue;
                    }

                    this.udpBufferWriter.EndWriting(result.IpEndPoint);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Unexpected exception in UDP receiver thread: {exception}");
            }
        }
    }
}
