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
  opaque DynamicOpaque<>;
  opaque DynamicLimitedOpaque<10>;
  opaque FixedLengthOpaque[10];
  UInt8 DynamicUInt8Array<>;
  UInt8 DynamicLimitedUInt8Array<10>;
  UInt8 FixedLengthUInt8Array[10];
  SimpleStruct DynamicSimpleStructArray<>;
  SimpleStruct DynamicLimitedSimpleStructArray<10>;
  SimpleStruct FixedLengthSimpleStructArray[10];
  SimpleEnum DynamicSimpleEnumArray<>;
  SimpleEnum DynamicLimitedSimpleEnumArray<10>;
  SimpleEnum FixedLengthSimpleEnumArray[10];
  StringType StringArray[10];
};

struct SimpleStruct
{
  Int32 Value;
};

program TestServiceProgram {
  version TestServiceVersion {
    void ThrowsException() = 1;
    int Echo(int value) = 2;
  } = 1;
  version TestServiceVersion2 {
    SimpleStruct SimpleStructSimpleStruct(SimpleStruct value) = 3;
  } = 2;
} = 0x20406080;
