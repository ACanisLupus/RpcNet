namespace RpcNet
{
    using System.Net;
    using RpcNet.Internal;

    public class Caller
    {
        public Caller(IPEndPoint ipEndPoint, Protocol protocol)
        {
            this.IpEndPoint = ipEndPoint;
            this.Protocol = protocol;
        }

        public IPEndPoint IpEndPoint { get; }
        public Protocol Protocol { get; }

        public override string ToString() => $"{Utilities.ConvertToString(this.Protocol)}:{this.IpEndPoint}";
    }
}
