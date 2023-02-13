// Copyright by Artur Wolf

namespace RpcNet.PortMapper;

using System.Net;

public class PortMapperServer : PortMapperServerStub
{
    private static readonly CallResult _callResult = new()
    {
        Port = 0,
        Result = Array.Empty<byte>()
    };

    private readonly List<Mapping> _mappings = new();

    public PortMapperServer(
        Protocol protocol,
        IPAddress ipAddress,
        int port = PortMapperConstants.PortMapperPort,
        ServerSettings serverSettings = default) : base(
        protocol,
        ipAddress,
        port,
        serverSettings)
    {
        lock (_mappings)
        {
            if (protocol.HasFlag(Protocol.Tcp))
            {
                _mappings.Add(
                    new Mapping
                    {
                        Port = port,
                        Program = PortMapperConstants.PortMapperProgram,
                        Protocol = ProtocolKind.Tcp,
                        Version = PortMapperConstants.PortMapperVersion
                    });
            }

            if (protocol.HasFlag(Protocol.Udp))
            {
                _mappings.Add(
                    new Mapping
                    {
                        Port = port,
                        Program = PortMapperConstants.PortMapperProgram,
                        Protocol = ProtocolKind.Udp,
                        Version = PortMapperConstants.PortMapperVersion
                    });
            }
        }
    }

    public override void Ping_2(Caller caller)
    {
    }

    public override bool Set_2(Caller caller, Mapping mapping)
    {
        lock (_mappings)
        {
            if (_mappings.Any(m => IsProgramAndVersionAndProtocolEqual(m, mapping)))
            {
                return false;
            }

            _mappings.Add(mapping);
            return true;
        }
    }

    public override bool Unset_2(Caller caller, Mapping mapping)
    {
        lock (_mappings)
        {
            Equal equal = IsProgramAndVersionAndProtocolEqual;
            if (mapping.Protocol == ProtocolKind.Unknown)
            {
                equal = IsProgramAndVersionEqual;
            }

            return _mappings.RemoveAll(tmpMapping => equal(tmpMapping, mapping)) > 0;
        }
    }

    public override int GetPort_2(Caller caller, Mapping mapping)
    {
        lock (_mappings)
        {
            Mapping found =
                _mappings.FirstOrDefault(m => IsProgramAndVersionAndProtocolEqual(m, mapping));
            return found?.Port ?? 0;
        }
    }

    public override MappingNodeHead Dump_2(Caller caller)
    {
        lock (_mappings)
        {
            var mappingNodeNullable = new MappingNodeHead();

            MappingNode currentNode = null;
            foreach (Mapping mapping in _mappings)
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

    public override CallResult Call_2(Caller caller, CallArguments callArguments) => _callResult;

    private static bool IsProgramAndVersionEqual(Mapping mapping1, Mapping mapping2) =>
        (mapping1.Program == mapping2.Program) && (mapping1.Version == mapping2.Version);

    private static bool IsProgramAndVersionAndProtocolEqual(
        Mapping mapping1,
        Mapping mapping2) =>
        IsProgramAndVersionEqual(mapping1, mapping2) && (mapping1.Protocol == mapping2.Protocol);

    private delegate bool Equal(Mapping mapping1, Mapping mapping2);
}
