namespace RpcNet.PortMapper
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using RpcNet.Internal;

    public class PortMapperClient : IDisposable
    {
        private readonly Internal.PortMapperClient client;

        public PortMapperClient(
            Protocol protocol,
            IPAddress ipAddress,
            PortMapperClientSettings portMapperClientSettings = default)
        {
            var settings = new ClientSettings();
            if (portMapperClientSettings != null)
            {
                settings.Port = portMapperClientSettings.Port;
                settings.Logger = portMapperClientSettings.Logger;
                settings.ReceiveTimeout = portMapperClientSettings.ReceiveTimeout;
                settings.SendTimeout = portMapperClientSettings.SendTimeout;
            }

            if (settings.Port == 0)
            {
                settings.Port = PortMapperConstants.PortMapperPort;
            }

            this.client = new Internal.PortMapperClient(protocol, ipAddress, settings);
        }

        public bool Unset(Mapping mapping)
        {
            if (mapping == null)
            {
                throw new ArgumentNullException(nameof(mapping));
            }

            return this.client.Unset_2(Convert(mapping));
        }

        public bool Set(Mapping mapping)
        {
            if (mapping == null)
            {
                throw new ArgumentNullException(nameof(mapping));
            }

            return this.client.Set_2(Convert(mapping));
        }

        public int GetPort(Mapping mapping)
        {
            if (mapping == null)
            {
                throw new ArgumentNullException(nameof(mapping));
            }

            return this.client.GetPort_2(Convert(mapping));
        }

        public IReadOnlyList<Mapping> Dump()
        {
            var list = new List<Mapping>();

            MappingNode node = this.client.Dump_2().MappingNode;
            while (node != null)
            {
                list.Add(Convert(node.Mapping));
                node = node.Next;
            }

            return list;
        }

        public void Dispose()
        {
            this.client.Dispose();
        }

        private static Protocol Convert(ProtocolKind protocol)
        {
            switch (protocol)
            {
                case ProtocolKind.Tcp:
                    return Protocol.Tcp;
                case ProtocolKind.Udp:
                    return Protocol.Udp;
                default:
                    throw new ArgumentOutOfRangeException(nameof(protocol), protocol, null);
            }
        }

        private static ProtocolKind Convert(Protocol protocol)
        {
            switch (protocol)
            {
                case Protocol.Tcp:
                    return ProtocolKind.Tcp;
                case Protocol.Udp:
                    return ProtocolKind.Udp;
                default:
                    throw new ArgumentOutOfRangeException(nameof(protocol), protocol, null);
            }
        }

        private static Mapping Convert(Internal.Mapping mapping) =>
            new Mapping
            {
                Protocol = Convert(mapping.Protocol),
                Port = mapping.Port,
                Program = mapping.Program,
                Version = mapping.Version
            };

        private static Internal.Mapping Convert(Mapping mapping) =>
            new Internal.Mapping
            {
                Protocol = Convert(mapping.Protocol),
                Port = mapping.Port,
                Program = mapping.Program,
                Version = mapping.Version
            };
    }
}
