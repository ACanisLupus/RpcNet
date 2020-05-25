//using System;
//using System.Net;
//using System.Net.Sockets;
//using System.Threading;

//namespace RpcNet.Internal
//{
//    internal class RpcUdpServer
//    {
//        private readonly byte[] buffer = new byte[65536];
//        private readonly UdpClient server;
//        private readonly Thread receiverThread;
//        private readonly XdrReader reader;
//        private readonly XdrWriter writer;

//        public RpcUdpServer(IPAddress ipAddress, int port)
//        {
//            this.server = new UdpClient(new IPEndPoint(ipAddress, port));
//            this.receiverThread = new Thread(this.DoReceiving);

//            reader = new XdrReader(this.buffer);
//            writer = new XdrWriter(this.buffer);
//        }

//        public void StartReceiving()
//        {
//            this.receiverThread.Start();
//        }

//        private void DoReceiving()
//        {
//            try
//            {
//                while (true)
//                {
//                    EndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Loopback, 0);
//                    int length = this.server.Client.ReceiveFrom(this.buffer, SocketFlags.None, ref remoteIpEndPoint);
//                    this.reader.Reset(length);

//                }
//            }
//            catch (ObjectDisposedException)
//            {
//            }
//            catch (SocketException)
//            {
//            }
//            catch (Exception exception)
//            {
//                Console.WriteLine($"Unexpected exception in UDP receiver thread: {exception}");
//            }
//        }
//    }
//}
