const PortMapperPort = 111;

enum ProtocolKind {
  Unknown = 0,
  Tcp = 6,
  Udp = 17
};

struct Mapping2 {
  int ProgramNumber;
  int VersionNumber;
  ProtocolKind Protocol;
  int Port;
};

struct MappingNode2 {
  Mapping2 Mapping;
  MappingNode2* Next;
};

typedef MappingNode2* MappingNodeHead2;

struct CallArguments {
  int ProgramNumber;
  int VersionNumber;
  int ProcedureNumber;
  opaque Arguments<>;
};

struct CallResult2 {
  int Port;
  opaque Result<>;
};

struct Mapping3 {
  unsigned int ProgramNumber;
  unsigned int VersionNumber;
  string NetworkId<>;
  string UniversalAddress<>;
  string OwnerOfThisService<>;
};

struct MappingNode3 {
  Mapping3 Mapping;
  MappingNode3* Next;
};

typedef MappingNode3* MappingNodeHead3;

struct CallResult3 {
  string RemoteUniversalAddress<>;
  opaque Result<>;
};

/*
 * netbuf structure, used to store the transport specific form of
 * a universal transport address.
 */
struct NetworkBuffer {
 unsigned int Maxlen;
 opaque Buffer<>;
};

struct Entry {
  string MergedAddressOfService<>;
  string NetworkId<>;
  unsigned int SemanticsOfTransport;
  string ProtocolFamily<>;
  string ProtocolName<>;
};

struct EntryNode {
  Entry Entry;
  EntryNode* Next;
};

typedef EntryNode* EntryNodeHead;

// Statistics

const HighestProcedurePlusOne = 13;  // # of procs in rpcbind V4 plus one
const StatisticsVersions = 3;  // provide only for rpcbind V2, V3 and V4

struct AddressStatistics {
  unsigned int ProgramNumber;
  unsigned int VersionNumber;
  int Success;
  int failure;
  string NetworkId<>;
  AddressStatistics* Next;
};

typedef AddressStatistics* AddressStatisticsHead;

struct RemoteCallStatistics {
  unsigned long ProgramNumber;
  unsigned long VersionNumber;
  unsigned long ProcedureNumber;
  int Success;
  int Failure;
  int Indirect;  // whether broadcast or indirect
  string NetworkId<>;
  RemoteCallStatistics* Next;
};

typedef RemoteCallStatistics* RemoteCallStatisticsHead;

typedef int ProcedureInfo[HighestProcedurePlusOne];

struct Statistics {
  ProcedureInfo Info;
  int SetInfo;
  int UnsetInfo;
  AddressStatisticsHead AddressInfo;
  RemoteCallStatisticsHead RemoteCallInfo;
};

typedef Statistics StatisticsByVersion[StatisticsVersions];

program PortMapperProgram {
  version PortMapperVersion2 {
    void Ping(void) = 0;
    bool Set(Mapping2) = 1;
    bool Unset(Mapping2) = 2;
    int GetPort(Mapping2) = 3;
    MappingNodeHead2 Dump(void) = 4;
    CallResult2 Call(CallArguments) = 5;
  } = 2;
  version PortMapperVersion3 {
    bool Set(Mapping3) = 1;
    bool Unset(Mapping3) = 2;
    string<> GetAddress(Mapping3) = 3;
    MappingNodeHead3 Dump(void) = 4;
    CallResult3 Call(CallArguments) = 5;
    unsigned int GetTime(void) = 6;
    NetworkBuffer UniversalAddressToTransportSpecificAddress(string universalAddress<>) = 7;
    string<> TransportSpecificAddressToUniversalAddress(NetworkBuffer) = 8;
  } = 3;
  version PortMapperVersion4 {
    bool Set(Mapping3) = 1;
    bool Unset(Mapping3) = 2;
    string<> GetAddress(Mapping3) = 3;
    MappingNodeHead3 Dump(void) = 4;
    CallResult3 Broadcast(CallArguments) = 5;
    unsigned int GetTime(void) = 6;
    NetworkBuffer UniversalAddressToTransportSpecificAddress(string universalAddress<>) = 7;
    string<> TransportSpecificAddressToUniversalAddress(NetworkBuffer) = 8;
    string<> GetVersionAddress(Mapping3) = 9;
    CallResult3 IndirectCall(CallArguments) = 10;
    EntryNodeHead GetAddressList(Mapping3) = 11;
    StatisticsByVersion GetStatistics(void) = 12;
  } = 4;
} = 100000;
