namespace RpcNet.PortMapper
{
    public class Mapping
    {
        public int Program { get; set; }
        public int Version { get; set; }
        public Protocol Protocol { get; set; } = Protocol.Tcp;
        public int Port { get; set; }

        public override string ToString() =>
            $"Protocol: {this.Protocol}, Program: {this.Program}, Version: {this.Version}, Port: {this.Port}";
    }
}
