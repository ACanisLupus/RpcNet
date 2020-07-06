// --------------------------------------------------------------------------------------------------------------------
// <copyright company="dSPACE GmbH" file="PortMapperServer.cs">
//   Copyright dSPACE GmbH. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace RpcNet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using RpcNet.Internal;

    public class PortMapperServer : IDisposable
    {
        private readonly PortMapperServerImpl server;

        public PortMapperServer(IPAddress ipAddress, ILogger logger = null)
        {
            this.server = new PortMapperServerImpl(ipAddress, logger);
        }

        public void Dispose() => this.server?.Dispose();

        private class PortMapperServerImpl : PortMapperServerStub
        {
            private readonly ILogger logger;
            private readonly List<Mapping> mappings = new List<Mapping>();

            public PortMapperServerImpl(IPAddress ipAddress, ILogger logger) : base(ipAddress, PortMapperConstants.PortMapperPort, logger)
            {
                this.logger = logger;
            }

            public override void Ping_2(IPEndPoint remoteIpEndPoint) => this.logger?.Info($"Received PING from {remoteIpEndPoint}.");

            public override bool Set_2(IPEndPoint remoteIpEndPoint, Mapping mapping)
            {
                this.logger?.Info($"[{remoteIpEndPoint}] SET     {ToLogString(mapping)}.");
                lock (this.mappings)
                {
                    if (this.mappings.Any(m => IsEqual(m, mapping)))
                    {
                        return false;
                    }

                    this.mappings.Add(mapping);
                    return true;
                }
            }

            public override bool Unset_2(IPEndPoint remoteIpEndPoint, Mapping mapping)
            {
                this.logger?.Info($"[{remoteIpEndPoint}] UNSET   {ToLogString(mapping)}.");
                lock (this.mappings)
                {
                    for (int i = this.mappings.Count - 1; i >= 0; i--)
                    {
                        if (IsEqualExceptPort(this.mappings[i], mapping))
                        {
                            this.mappings.RemoveAt(i);
                            return true;
                        }
                    }

                    return false;
                }
            }

            public override int GetPort_2(IPEndPoint remoteIpEndPoint, Mapping mapping)
            {
                this.logger?.Info($"[{remoteIpEndPoint}] GETPORT {ToLogString(mapping)}.");
                lock (this.mappings)
                {
                    Mapping found = this.mappings.FirstOrDefault(m => IsEqualExceptPort(m, mapping));
                    if (found != null)
                    {
                        return found.Port;
                    }

                    return 0;
                }
            }

            public override MappingList Dump_2(IPEndPoint remoteIpEndPoint)
            {
                this.logger?.Info($"{remoteIpEndPoint} DUMP.");
                lock (this.mappings)
                {
                    MappingList firstItem = null;

                    MappingList currentItem = null;
                    foreach (Mapping mapping in this.mappings)
                    {
                        var newItem = new MappingList { Mapping = mapping };
                        if (currentItem == null)
                        {
                            currentItem = newItem;
                        }
                        else
                        {
                            currentItem.Next = newItem;
                        }

                        if (firstItem == null)
                        {
                            firstItem = newItem;
                        }
                    }

                    return firstItem;
                }
            }

            public override CallResult Call_2(IPEndPoint remoteIpEndPoint, CallArguments arg1)
            {
                this.logger?.Info($"{remoteIpEndPoint} CALLIT.");
                return new CallResult();
            }

            private static bool IsEqual(Mapping mapping1, Mapping mapping2) => IsEqualExceptPort(mapping1, mapping2) && (mapping1.Port == mapping2.Port);

            private static bool IsEqualExceptPort(Mapping mapping1, Mapping mapping2)
                => (mapping1.Program == mapping2.Program) && (mapping1.Protocol == mapping2.Protocol) && (mapping1.Version == mapping2.Version);

            private static string ToLogString(Mapping mapping)
                => $"(Port: {mapping.Port}, Program: {mapping.Program}, Protocol: {mapping.Protocol}, Version: {mapping.Version})";
        }
    }
}
