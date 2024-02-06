grammar Rpc;

// parser rules

bool: 'bool' | 'boolean';
uint8: 'unsigned' 'char' | 'uint8' | 'UInt8' | 'byte';
uint16: 'unsigned' 'short' | 'uint16' | 'UInt16';
uint32: 'unsigned' 'int' | 'unsigned' 'long' | 'uint32' | 'UInt32';
uint64: 'unsigned' 'hyper' | 'uint64' | 'UInt64';
int8: 'char' | 'int8' | 'Int8' | 'sbyte';
int16: 'short' | 'int16' | 'Int16';
int32: 'int' | 'long' | 'int32' | 'Int32';
int64: 'hyper' | 'int64' | 'Int64';
float32: 'float' | 'float32' | 'Float32';
float64: 'double' | 'float64' | 'Float64';
void: 'void';

dataType: bool | uint8 | uint16 | uint32 | uint64 | int8 | int16 | int32 | int64 | float32 | float64 | Identifier;

opaque: 'opaque' Identifier '<' value? '>' | 'opaque' '<' value? '>' Identifier?;
string: 'string' Identifier '<' value? '>' | 'string' '<' value? '>' Identifier?;
scalar: dataType Identifier?;
pointer: dataType '*' Identifier?;
array: dataType Identifier '[' value ']' | dataType '[' value ']' Identifier?;
vector: dataType Identifier '<' value? '>' | dataType '<' value? '>' Identifier?;

declaration: opaque | string | scalar | pointer | array | vector;

value: constant | Identifier;
constant: Decimal | Hexadecimal | Octal;

enum: 'enum' Identifier '{' (enumValue) (',' enumValue)* ','? '}' ';';
enumValue: Identifier '=' value;

struct: 'struct' Identifier '{' (declaration ';')+ '}' ';';

union: 'union' Identifier 'switch' '(' declaration ')' '{' case+ ('default' ':' defaultItem ';')? '}' ';';
case: ('case' value ':') unionItem ';';
unionItem: declaration | void;
defaultItem: declaration | void;

const: 'const' Identifier '=' constant ';';

typedef: 'typedef' declaration ';';

definition: typedef | enum | struct | union | const;

program: 'program' Identifier '{' version+ '}' '=' constant ';';

version: 'version' Identifier '{' procedure+ '}' '=' constant ';';

procedure: return Identifier '(' arguments? ')' '=' constant ';';

return: void | declaration;
argumentList: declaration (',' declaration)*;
arguments: void | argumentList;

rpcSpecification: (definition | program)*;

// lexer rules

BlockComment: '/*' .*? '*/' -> skip;
LineComment: '//' ~('\r' | '\n')* -> skip;
Octal: '0' [1-7] ([0-7])*;
Decimal: ('-')? ([0-9])+;
Hexadecimal: '0x' ([a-fA-F0-9])+;
Identifier: [a-zA-Z] ([a-zA-Z0-9_])*;
Ws: [ \t\r\n]+ -> skip;
