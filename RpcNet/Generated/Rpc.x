struct AcceptedReply {
  OpaqueAuthentication Verifier;
  ReplyData ReplyData;
};

enum AcceptStatus {
  Success = 0,
  ProgramUnavailable = 1, /* remote hasn't exported program */
  ProgramMismatch = 2, /* remote can't support version # */
  ProcedureUnavailable = 3, /* program can't support procedure */
  GarbageArguments = 4, /* procedure can't decode params */
  SystemError = 5 /* e.g. memory allocation failure */
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
  /*
   * failed at remote end
   */
  BadCredential = 1, /* bad credential (seal broken) */
  RejectedCredential = 2, /* client must begin new session */
  BadVerifier = 3, /* bad verifier (seal broken) */
  RejectedVerifier = 4, /* verifier expired or replayed */
  TooWeak = 5, /* rejected for security reasons */
  /*
   * failed locally
   */
  InvalidResponseVerifier = 6,
  FailedUnknownReason = 7,
  /*
   * AUTH_KERB errors; deprecated.  See [RFC2695]
   */
  KerberosGenericError = 8,
  TimeOfCredentialExpired = 9,
  ProblemWithTicketFile = 10,
  FailedToDecodeAuthenticator = 11,
  InvalidNetAddress = 12,
  /*
   * RPCSEC_GSS GSS related errors
   */
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
  unsigned int RpcVersion; /* must be equal to two (2) */
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
  RpcVersionMismatch = 0, /* RPC version number != 2 */
  AuthenticationError = 1 /* remote can't authenticate caller */
};

union ReplyBody switch (ReplyStatus ReplyStatus) {
  case Accepted:
    AcceptedReply AcceptedReply;
  case Denied:
    RejectedReply RejectedReply;
};

union ReplyData switch (AcceptStatus AcceptStatus) {
  case Success:
    /*opaque results[0];*/
    /*
     * procedure-specific results start here
     */
    void;
  case ProgramMismatch:
    MismatchInfo MismatchInfo;
  default:
    /*
     * Void.  Cases include ProgramUnavailable, ProcedureUnavailable,
     * GarbageArguments, and SystemError.
     */
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
