// This interface uses the same program number but slightly different version numbers and procedure numbers.
// This interface is used to handle protocol errors correctly.
program TestServiceProgram2 {
  version TestServiceVersion {
    void ThrowsException() = 1;
    int Echo(int value) = 2;
    void NonExistingProcedure(opaque someBytes<>) = 3;
  } = 1;
  version NonExistingVersion {
    void NonExistingProcedure(opaque someBytes<>) = 3;
  } = 3;
} = 0x20406080;
