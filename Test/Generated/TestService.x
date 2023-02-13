enum SimpleEnum
{
  Value1,
  Value2 = 1
};

typedef string StringType<>;

struct ComplexStruct
{
  bool BoolValue;
  Int8 Int8Value;
  Int16 Int16Value;
  Int32 Int32Value;
  Int64 Int64Value;
  UInt8 UInt8Value;
  UInt16 UInt16Value;
  UInt32 UInt32Value;
  UInt64 UInt64Value;
  float Float32Value;
  double Float64Value;
  SimpleStruct SimpleStructValue;
  SimpleEnum SimpleEnumValue;
  UInt8 UInt8DynamicArray<>;
  SimpleStruct SimpleStructDynamicArray<>;
  SimpleEnum SimpleEnumDynamicArray<>;
  double Float64FixedArray[10];
  SimpleStruct SimpleStructFixedArray[10];
  SimpleEnum SimpleEnumFixedArray[10];
  StringType StringArray[2];
};

struct SimpleStruct
{
  Int32 Value;
};

program TestServiceProgram {
  version TestServiceVersion {
    void VoidVoid1() = 1;
    void VoidVoid2(void) = 2;

    int IntInt1(int value) = 3;
    int IntInt2(int) = 4;
  } = 1;
  version TestServiceVersion2 {
    SimpleStruct SimpleStructSimpleStruct(SimpleStruct value) = 7;
  } = 2;
} = 0x020406080;
