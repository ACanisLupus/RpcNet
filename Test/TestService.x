struct MyStruct
{
  bool BoolValue;
  char Int8Value;
  Int16 Int16Value;
  Int32 Int32Value;
  Int64 Int64Value;
  UInt16 UInt16Value;
  UInt32 UInt32Value;
  UInt64 UInt64Value;
  double Float64Value;
  float Float32Value;
  bool BoolValue2<>;
  char Int8Value2<>;
  Int16 Int16Value2<>;
  Int32 Int32Value2<>;
  Int64 Int64Value2<>;
  UInt16 UInt16Value2<>;
  UInt32 UInt32Value2<>;
  UInt64 UInt64Value2<>;
  double Float64Value2<>;
  float Float32Value2<>;
  bool BoolValue3[10];
  char Int8Value3[10];
  Int16 Int16Value3[10];
  Int32 Int32Value3[10];
  Int64 Int64Value3[10];
  UInt16 UInt16Value3[10];
  UInt32 UInt32Value3[10];
  UInt64 UInt64Value3[10];
  double Float64Value3[10];
  float Float32Value3[10];
};

struct PingStruct
{
  Int32 Value;
};

program TestServiceProgram {
  version TestServiceVersion {
    PingStruct Ping(PingStruct) = 1;
    MyStruct TestMyStruct(MyStruct) = 2;
  } = 1;
} = 0x02004009;
