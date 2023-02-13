const PortMapperPort = 111;

enum ProtocolKind {
  Unknown = 0,
  Tcp = 6,
  Udp = 17
};

struct Mapping {
  int Program;
  int Version;
  ProtocolKind Protocol;
  int Port;
};

/*
struct* cannot be generated yet. Therefore the workaround with MappingNodeHead
struct* MappingNode {
  Mapping Mapping;
  MappingNode Next;
};
*/

struct MappingNodeHead {
  MappingNode* MappingNode;
};

struct MappingNode {
  Mapping Mapping;
  MappingNode* Next;
};

struct CallArguments {
  int Program;
  int Version;
  int Procedure;
  opaque Arguments<>;
};

struct CallResult {
  int Port;
  opaque Result<>;
};

program PortMapperProgram {
  version PortMapperVersion {
    void Ping(void) = 0;
    bool Set(Mapping) = 1;
    bool Unset(Mapping) = 2;
    int GetPort(Mapping) = 3;
    MappingNodeHead Dump(void) = 4;
    CallResult Call(CallArguments) = 5;
  } = 2;
} = 100000;
