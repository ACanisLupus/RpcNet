const PortMapperPort = 111;

enum ProtocolKind {
  Tcp = 6,
  Udp = 17
};

struct Mapping {
  int Program;
  int Version;
  ProtocolKind Protocol;
  int Port;
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
    MappingNode Dump(void) = 4;
    CallResult Call(CallArguments) = 5;
  } = 2;
} = 100000;
