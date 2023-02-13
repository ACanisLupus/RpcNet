struct AcceptedReply {
  OpaqueAuthentication Verifier;
  ReplyData ReplyData;
};

enum AcceptStatus {
  Success = 0,
  ProgramUnavailable = 1,
  ProgramMismatch = 2,
  ProcedureUnavailable = 3,
  GarbageArguments = 4,
  SystemError = 5
};

enum AuthenticationFlavor {
  None = 0,
  Unix = 1,
  Short = 2,
  Des = 3,
  Gss = 6
};

enum AuthenticationStatus {
  Ok = 0,
  BadCredential = 1,
  RejectedCredential = 2,
  BadVerifier = 3,
  RejectedVerifier = 4,
  TooWeak = 5,
  InvalidResponseVerifier = 6,
  FailedUnknownReason = 7,
  KerberosGenericError = 8,
  TimeOfCredentialExpired = 9,
  ProblemWithTicketFile = 10,
  FailedToDecodeAuthenticator = 11,
  InvalidNetAddress = 12,
  GssMissingCredential = 13,
  GssContextProblem = 14
};

union Body switch (MessageType MessageType) {
  case Call:
    CallBody CallBody;
  case Reply:
    ReplyBody ReplyBody;
};

struct CallBody {
  unsigned int RpcVersion;
  unsigned int Program;
  unsigned int Version;
  unsigned int Procedure;
  OpaqueAuthentication Credential;
  OpaqueAuthentication Verifier;
  /* procedure-specific parameters start here */
};

enum MessageType {
  Call = 0,
  Reply = 1
};

struct MismatchInfo {
  unsigned int Low;
  unsigned int High;
};

struct OpaqueAuthentication {
  AuthenticationFlavor AuthenticationFlavor;
  opaque Body<400>;
};

union RejectedReply switch (RejectStatus RejectStatus) {
  case RpcVersionMismatch:
    MismatchInfo MismatchInfo;
  case AuthenticationError:
    AuthenticationStatus AuthenticationStatus;
};

enum RejectStatus {
  RpcVersionMismatch = 0,
  AuthenticationError = 1
};

union ReplyBody switch (ReplyStatus ReplyStatus) {
  case Accepted:
    AcceptedReply AcceptedReply;
  case Denied:
    RejectedReply RejectedReply;
};

union ReplyData switch (AcceptStatus AcceptStatus) {
  case Success:
    void;
  case ProgramMismatch:
    MismatchInfo MismatchInfo;
  default:
    void;
};

enum ReplyStatus {
  Accepted = 0,
  Denied = 1
};

struct RpcMessage {
  unsigned int Xid;
  Body Body;
};
