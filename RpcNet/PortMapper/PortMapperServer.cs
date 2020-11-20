namespace RpcNet.PortMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using RpcNet.Internal;

    public class PortMapperServer : IDisposable
    {
        private readonly PortMapperServerImpl server;

        public PortMapperServer(Protocol protocol, IPAddress ipAddress, PortMapperServerSettings portMapperServerSettings = default)
        {
            var settings = new ServerSettings();
            if (portMapperServerSettings != null)
            {
                settings.Port = portMapperServerSettings.Port;
                settings.ReceiveTimeout = portMapperServerSettings.ReceiveTimeout;
                settings.SendTimeout = portMapperServerSettings.SendTimeout;
                settings.Logger = portMapperServerSettings.Logger;
            }

            if (settings.Port == 0)
            {
                settings.Port = PortMapperConstants.PortMapperPort;
            }

            this.server = new PortMapperServerImpl(protocol, ipAddress, settings);
        }

        public void Start()
        {
            this.server.Start();
        }

        public void Dispose()
        {
            this.server.Dispose();
        }

        private class PortMapperServerImpl : PortMapperServerStub
        {
            private readonly ILogger logger;
            private readonly List<Internal.Mapping> mappings = new List<Internal.Mapping>();

            public PortMapperServerImpl(Protocol protocol, IPAddress ipAddress, ServerSettings serverSettings) : base(
                protocol,
                ipAddress,
                serverSettings)
            {
                this.logger = serverSettings?.Logger;

                lock (this.mappings)
                {
                    if (protocol.HasFlag(Protocol.Tcp))
                    {
                        this.mappings.Add(new Internal.Mapping
                        {
                            Port = serverSettings?.Port ?? PortMapperConstants.PortMapperPort,
                            Program = PortMapperConstants.PortMapperProgram,
                            Protocol = ProtocolKind.Tcp,
                            Version = PortMapperConstants.PortMapperVersion
                        });
                    }

                    if (protocol.HasFlag(Protocol.Udp))
                    {
                        this.mappings.Add(new Internal.Mapping
                        {
                            Port = serverSettings?.Port ?? PortMapperConstants.PortMapperPort,
                            Program = PortMapperConstants.PortMapperProgram,
                            Protocol = ProtocolKind.Udp,
                            Version = PortMapperConstants.PortMapperVersion
                        });
                    }
                }
            }

            public override void Ping_2(Caller caller)
            {
                this.logger?.Info($"Received PING from {caller}.");
            }

            public override bool Set_2(Caller caller, Internal.Mapping mapping)
            {
                this.logger?.Info($"{caller} SET     {ToLogString(mapping)}.");
                lock (this.mappings)
                {
                    if (this.mappings.Any(m => IsProgramAndVersionAndProtocolEqual(m, mapping)))
                    {
                        return false;
                    }

                    this.mappings.Add(mapping);
                    return true;
                }
            }

            private delegate bool Equal(Internal.Mapping mapping1, Internal.Mapping mapping2);

            public override bool Unset_2(Caller caller, Internal.Mapping mapping)
            {
                this.logger?.Info($"{caller} UNSET   {ToLogString(mapping)}.");
                lock (this.mappings)
                {
                    Equal equal = IsProgramAndVersionAndProtocolEqual;
                    if (mapping.Protocol == ProtocolKind.Unknown)
                    {
                        equal = IsProgramAndVersionEqual;
                    }

                    return this.mappings.RemoveAll(tmpMapping => equal(tmpMapping, mapping)) > 0;
                }
            }

            public override int GetPort_2(Caller caller, Internal.Mapping mapping)
            {
                this.logger?.Info($"{caller} GETPORT {ToLogString(mapping)}.");
                lock (this.mappings)
                {
                    Internal.Mapping found =
                        this.mappings.FirstOrDefault(m => IsProgramAndVersionAndProtocolEqual(m, mapping));
                    if (found != null)
                    {
                        return found.Port;
                    }

                    return 0;
                }
            }

            public override MappingNodeHead Dump_2(Caller caller)
            {
                this.logger?.Info($"{caller} DUMP.");
                lock (this.mappings)
                {
                    var mappingNodeNullable = new MappingNodeHead();

                    MappingNode currentNode = null;
                    foreach (Internal.Mapping mapping in this.mappings)
                    {
                        if (mappingNodeNullable.MappingNode == null)
                        {
                            mappingNodeNullable.MappingNode = new MappingNode { Mapping = mapping };
                            currentNode = mappingNodeNullable.MappingNode;
                        }
                        else if (currentNode != null)
                        {
                            var mappingNode = new MappingNode { Mapping = mapping };

                            currentNode.Next = mappingNode;
                            currentNode = mappingNode;
                        }
                    }

                    return mappingNodeNullable;
                }
            }

            public override CallResult Call_2(Caller caller, CallArguments arg1)
            {
                this.logger?.Info($"{caller} CALL.");
                return new CallResult();
            }

            private static bool IsProgramAndVersionEqual(Internal.Mapping mapping1, Internal.Mapping mapping2) =>
                (mapping1.Program == mapping2.Program) && (mapping1.Version == mapping2.Version);

            private static bool IsProgramAndVersionAndProtocolEqual(
                Internal.Mapping mapping1,
                Internal.Mapping mapping2) =>
                IsProgramAndVersionEqual(mapping1, mapping2) && (mapping1.Protocol == mapping2.Protocol);

            private static string ToLogString(Internal.Mapping mapping) =>
                $"(Port: {mapping.Port}, Program: {mapping.Program}, " +
                $"Protocol: {mapping.Protocol}, Version: {mapping.Version})";
        }
    }
}
