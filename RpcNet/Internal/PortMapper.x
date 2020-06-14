const PortMapperPort = 111;
const ProtocolTcp = 6;
const ProtocolUdp = 17;

struct Mapping {
  unsigned int Program;
  unsigned int Version;
  unsigned int Protocol;
  unsigned int Port;
};

struct PortMapperList {
  Mapping Mapping;
  PortMapperList* Next;
};

struct CallArguments {
  unsigned int Program;
  unsigned int Version;
  unsigned int Procedure;
  opaque Arguments<>;
};

struct CallResult {
  unsigned int Port;
  opaque Result<>;
};

program PortMapperProgram {
  version PortMapperVersion {
    void Ping(void) = 0;
    bool Set(Mapping) = 1;
    bool Unset(Mapping) = 2;
    unsigned int GetPort(Mapping) = 3;
    PortMapperList Dump(void) = 4;
    CallResult Call(CallArguments) = 5;
  } = 2;
} = 100000;
