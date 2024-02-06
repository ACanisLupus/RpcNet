// Copyright by Artur Wolf

namespace RpcNet.PortMapper;

using System.Net;

public sealed class PortMapperServer : PortMapperServerStub
{
    private static readonly CallResult2 _callResult = new() { Port = 0, Result = Array.Empty<byte>() };

    private readonly List<Mapping2> _mappings2 = new();
    private readonly List<Mapping3> _mappings3 = new();

    public PortMapperServer(
        Protocol protocol,
        IPAddress ipAddress,
        int port = PortMapperConstants.PortMapperPort,
        ServerSettings? serverSettings = default) : base(
        protocol,
        ipAddress,
        port,
        serverSettings)
    {
        if (protocol.HasFlag(Protocol.Tcp))
        {
            _mappings2.Add(
                new Mapping2
                {
                    Port = port,
                    ProgramNumber = PortMapperConstants.PortMapperProgram,
                    Protocol = ProtocolKind.Tcp,
                    VersionNumber = PortMapperConstants.PortMapperVersion2
                });
        }

        if (protocol.HasFlag(Protocol.Udp))
        {
            _mappings2.Add(
                new Mapping2
                {
                    Port = port,
                    ProgramNumber = PortMapperConstants.PortMapperProgram,
                    Protocol = ProtocolKind.Udp,
                    VersionNumber = PortMapperConstants.PortMapperVersion2
                });
        }
    }

    public override void Ping_2(Caller caller)
    {
    }

    public override bool Set_2(Caller caller, Mapping2 mapping)
    {
        if (_mappings2.Any(m => IsProgramAndVersionAndProtocolEqual(m, mapping)))
        {
            return false;
        }

        _mappings2.Add(mapping);
        return true;
    }

    public override bool Unset_2(Caller caller, Mapping2 mapping)
    {
        Equal2 equal = IsProgramAndVersionAndProtocolEqual;
        if (mapping.Protocol == ProtocolKind.Unknown)
        {
            equal = IsProgramAndVersionEqual;
        }

        return _mappings2.RemoveAll(tmpMapping => equal(tmpMapping, mapping)) > 0;
    }

    public override int GetPort_2(Caller caller, Mapping2 mapping2)
    {
        Mapping2? found2 = _mappings2.FirstOrDefault(m => IsProgramAndVersionAndProtocolEqual(m, mapping2));
        if (found2 is not null)
        {
            return found2.Port;
        }

        Mapping3 mapping3 = Convert(mapping2);
        Mapping3? found3 = _mappings3.FirstOrDefault(m => IsProgramAndVersionAndProtocolEqual(m, mapping3));
        if (found3 is not null)
        {
            return ConvertUniversalAddressToPort(found3.UniversalAddress);
        }

        return 0;
    }

    public override MappingNodeHead2 Dump_2(Caller caller)
    {
        var mappingNodeNullable = new MappingNodeHead2();

        MappingNode2? currentNode = null;
        foreach (Mapping2 mapping in _mappings2)
        {
            if (mappingNodeNullable.Value is null)
            {
                mappingNodeNullable.Value = new MappingNode2 { Mapping = mapping };
                currentNode = mappingNodeNullable.Value;
            }
            else if (currentNode is not null)
            {
                var mappingNode = new MappingNode2 { Mapping = mapping };

                currentNode.Next = mappingNode;
                currentNode = mappingNode;
            }
        }

        return mappingNodeNullable;
    }

    public override CallResult2 Call_2(Caller caller, CallArguments callArguments) => _callResult;

    public override bool Set_3(Caller caller, Mapping3 mapping3)
    {
        if (_mappings3.Any(m => IsProgramAndVersionAndProtocolEqual(m, mapping3)))
        {
            return false;
        }

        _mappings3.Add(mapping3);
        return true;
    }

    public override bool Unset_3(Caller caller, Mapping3 mapping3)
    {
        Equal3 equal = IsProgramAndVersionAndProtocolEqual;

        return _mappings3.RemoveAll(tmpMapping => equal(tmpMapping, mapping3)) > 0;
    }

    public override string GetAddress_3(Caller caller, Mapping3 mapping3)
    {
        Mapping3? found3 = _mappings3.FirstOrDefault(m => IsProgramAndVersionAndProtocolEqual(m, mapping3));
        if (found3 is not null)
        {
            return found3.UniversalAddress;
        }

        Mapping2 mapping2 = Convert(mapping3);
        Mapping2? found2 = _mappings2.FirstOrDefault(m => IsProgramAndVersionAndProtocolEqual(m, mapping2));
        if (found2 is not null)
        {
            return ConvertPortToUniversalAddress(found2.Port);
        }

        return string.Empty;
    }

    public override MappingNodeHead3 Dump_3(Caller caller) => throw new NotImplementedException();
    public override CallResult3 Call_3(Caller caller, CallArguments callArguments) => throw new NotImplementedException();
    public override uint GetTime_3(Caller caller) => throw new NotImplementedException();
    public override NetworkBuffer UniversalAddressToTransportSpecificAddress_3(Caller caller, string universalAddress) => throw new NotImplementedException();
    public override string TransportSpecificAddressToUniversalAddress_3(Caller caller, NetworkBuffer networkBuffer) => throw new NotImplementedException();
    public override bool Set_4(Caller caller, Mapping3 mapping3) => Set_3(caller, mapping3);
    public override bool Unset_4(Caller caller, Mapping3 mapping3) => Unset_3(caller, mapping3);
    public override string GetAddress_4(Caller caller, Mapping3 mapping3) => GetAddress_3(caller, mapping3);
    public override MappingNodeHead3 Dump_4(Caller caller) => throw new NotImplementedException();
    public override CallResult3 Broadcast_4(Caller caller, CallArguments callArguments) => throw new NotImplementedException();
    public override uint GetTime_4(Caller caller) => throw new NotImplementedException();
    public override NetworkBuffer UniversalAddressToTransportSpecificAddress_4(Caller caller, string universalAddress) => throw new NotImplementedException();
    public override string TransportSpecificAddressToUniversalAddress_4(Caller caller, NetworkBuffer networkBuffer) => throw new NotImplementedException();
    public override string GetVersionAddress_4(Caller caller, Mapping3 mapping3) => throw new NotImplementedException();
    public override CallResult3 IndirectCall_4(Caller caller, CallArguments callArguments) => throw new NotImplementedException();
    public override EntryNodeHead GetAddressList_4(Caller caller, Mapping3 mapping3) => throw new NotImplementedException();
    public override StatisticsByVersion GetStatistics_4(Caller caller) => throw new NotImplementedException();

    private static bool IsProgramAndVersionEqual(Mapping2 firstMapping, Mapping2 secondMapping) =>
        (firstMapping.ProgramNumber == secondMapping.ProgramNumber) && (firstMapping.VersionNumber == secondMapping.VersionNumber);

    private static bool IsProgramAndVersionEqual(Mapping3 firstMapping, Mapping3 secondMapping) =>
        (firstMapping.ProgramNumber == secondMapping.ProgramNumber) && (firstMapping.VersionNumber == secondMapping.VersionNumber);

    private static bool IsProgramAndVersionAndProtocolEqual(Mapping2 firstMapping, Mapping2 secondMapping) =>
        IsProgramAndVersionEqual(firstMapping, secondMapping) && (firstMapping.Protocol == secondMapping.Protocol);

    private static bool IsProgramAndVersionAndProtocolEqual(Mapping3 firstMapping, Mapping3 secondMapping) =>
        IsProgramAndVersionEqual(firstMapping, secondMapping) && (firstMapping.NetworkId == secondMapping.NetworkId);

    private static Mapping2 Convert(Mapping3 mapping3) =>
        new()
        {
            ProgramNumber = (int)mapping3.ProgramNumber,
            VersionNumber = (int)mapping3.VersionNumber,
            Protocol = ConvertStringToProtocolKind(mapping3.NetworkId),
            Port = ConvertUniversalAddressToPort(mapping3.UniversalAddress)
        };

    private static Mapping3 Convert(Mapping2 mapping2) =>
        new()
        {
            ProgramNumber = (uint)mapping2.ProgramNumber,
            VersionNumber = (uint)mapping2.VersionNumber,
            NetworkId = ConvertProtocolToString(mapping2.Protocol),
            UniversalAddress = ConvertPortToUniversalAddress(mapping2.Port)
        };

    private static ProtocolKind ConvertStringToProtocolKind(string protocol)
    {
        if (protocol.Equals("tcp", StringComparison.InvariantCultureIgnoreCase))
        {
            return ProtocolKind.Tcp;
        }

        if (protocol.Equals("udp", StringComparison.InvariantCultureIgnoreCase))
        {
            return ProtocolKind.Udp;
        }

        return ProtocolKind.Unknown;
    }

    private static string ConvertProtocolToString(ProtocolKind protocolKind) =>
        protocolKind switch
        {
            ProtocolKind.Tcp => "tcp",
            ProtocolKind.Udp => "udp",
            _ => ""
        };

    private static int ConvertUniversalAddressToPort(string universalAddress)
    {
        string[] items = universalAddress.Split('.');
        if (items.Length < 2)
        {
            return 0;
        }

        string lowPartString = items[^1];
        string highPartString = items[^2];

        if (!int.TryParse(lowPartString, out int lowPart) || !int.TryParse(highPartString, out int highPart))
        {
            return 0;
        }

        int port = (highPart << 8) + lowPart;

        return port;
    }

    private static string ConvertPortToUniversalAddress(int port)
    {
        if (port == 0)
        {
            return "";
        }

        int highPart = (port & 0xff00) >> 8;
        int lowPart = port & 0xff;

        return $"0.0.0.0.{highPart}.{lowPart}";
    }

    private delegate bool Equal2(Mapping2 firstMapping, Mapping2 secondMapping);
    private delegate bool Equal3(Mapping3 firstMapping, Mapping3 secondMapping);
}
