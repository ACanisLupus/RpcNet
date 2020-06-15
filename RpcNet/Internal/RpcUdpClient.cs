namespace RpcNet.Internal
{
    using System.Net;
    using System.Net.Sockets;

    public class RpcUdpClient : INetworkClient
    {
        private readonly IPEndPoint remoteIpEndPoint;
        private readonly UdpClient client;
        //private readonly UdpServiceReader udpBufferReader;
        //private readonly UdpServiceWriter udpBufferWriter;
        private readonly Call call;

        public RpcUdpClient(IPAddress ipAddress, int port, uint program)
        {
            this.remoteIpEndPoint = new IPEndPoint(ipAddress, port);
            this.client = new UdpClient();
            //this.udpBufferReader = new UdpServiceReader(this.client.Client);
            //this.udpBufferWriter = new UdpServiceWriter(this.client.Client);
            //this.call = new Call(program, this.udpBufferReader, this.udpBufferWriter);
        }

        public int TimeoutInMilliseconds { get; set; }

        public void Call(uint procedure, uint version, IXdrWritable argument, IXdrReadable result)
        {
            //this.udpBufferWriter.BeginWriting();
            //this.call.SendCall(procedure, version, argument);
            //UdpResult udpResult = this.udpBufferWriter.EndWriting(this.remoteIpEndPoint);
            //if (udpResult.SocketError != SocketError.Success || udpResult.BytesLength == 0)
            //{
            //    throw new RpcException($"Could not send UDP message to {this.remoteIpEndPoint}. Socket error: {udpResult.SocketError}, transferred bytes: {udpResult.BytesLength}.");
            //}

            //udpResult = this.udpBufferReader.BeginReading(this.TimeoutInMilliseconds);
            //if (udpResult.SocketError != SocketError.Success || udpResult.BytesLength == 0)
            //{
            //    throw new RpcException($"Could not receive UDP reply from {this.remoteIpEndPoint}. Socket error: {udpResult.SocketError}, transferred bytes: {udpResult.BytesLength}.");
            //}

            //this.call.ReceiveResult(result);
        }
    }
}
