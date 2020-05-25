enum AuthenticationFlavor {
    AUTH_NONE       = 0,
    AUTH_SYS        = 1,
    AUTH_SHORT      = 2,
    AUTH_DH         = 3,
    RPCSEC_GSS      = 6
    /* and more to be defined */
};

struct OpaqueAuthentication {
    AuthenticationFlavor AuthenticationFlavor;
    opaque Body<400>;
};

enum MessageType {
    Call  = 0,
    Reply = 1
};

enum ReplyStatus {
    Accepted = 0,
    Denied   = 1
};

enum AcceptStatus {
    Success              = 0, /* RPC executed successfully       */
    ProgramUnavailable   = 1, /* remote hasn't exported program  */
    ProgramMismatch      = 2, /* remote can't support version #  */
    ProcedureUnavailable = 3, /* program can't support procedure */
    GarbageArguments     = 4, /* procedure can't decode params   */
    SystemError          = 5  /* e.g. memory allocation failure  */
};

enum RejectStatus {
    RpcVersionMismatch = 0, /* RPC version number != 2          */
    AuthenticationError = 1 /* remote can't authenticate caller */
};

enum AuthenticationStatus {
    AUTH_OK           = 0,  /* success                        */
    /*
     * failed at remote end
     */
    AUTH_BADCRED      = 1,  /* bad credential (seal broken)   */
    AUTH_REJECTEDCRED = 2,  /* client must begin new session  */
    AUTH_BADVERF      = 3,  /* bad verifier (seal broken)     */
    AUTH_REJECTEDVERF = 4,  /* verifier expired or replayed   */
    AUTH_TOOWEAK      = 5,  /* rejected for security reasons  */
    /*
     * failed locally
     */
    AUTH_INVALIDRESP  = 6,  /* bogus response verifier        */
    AUTH_FAILED       = 7,  /* reason unknown                 */
    /*
     * AUTH_KERB errors; deprecated.  See [RFC2695]
     */
    AUTH_KERB_GENERIC = 8,  /* kerberos generic error */
    AUTH_TIMEEXPIRE = 9,    /* time of credential expired */
    AUTH_TKT_FILE = 10,     /* problem with ticket file */
    AUTH_DECODE = 11,       /* can't decode authenticator */
    AUTH_NET_ADDR = 12,     /* wrong net address in ticket */
    /*
     * RPCSEC_GSS GSS related errors
     */
    RPCSEC_GSS_CREDPROBLEM = 13, /* no credentials for user */
    RPCSEC_GSS_CTXPROBLEM = 14   /* problem with context */
};

union Body switch (MessageType MessageType) {
    case Call:
        CallBody CallBody;
    case Reply:
        ReplyBody ReplyBody;
};

struct RpcMessage {
    unsigned int Xid;
    Body Body;
};

struct CallBody {
    unsigned int RpcVersion;       /* must be equal to two (2) */
    unsigned int Program;
    unsigned int Version;
    unsigned int Procedure;
    OpaqueAuthentication Credential;
    OpaqueAuthentication Verifier;
    /* procedure-specific parameters start here */
};

union ReplyBody switch (ReplyStatus ReplyStatus) {
    case Accepted:
        AcceptedReply AcceptedReply;
    case Denied:
        RejectedReply RejectedReply;
};

struct MismatchInfo {
    unsigned int Low;
    unsigned int High;
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

struct AcceptedReply {
    OpaqueAuthentication Verifier;
    ReplyData ReplyData;
};

union RejectedReply switch (RejectStatus RejectStatus) {
    case RpcVersionMismatch:
        MismatchInfo MismatchInfo;
    case AuthenticationError:
        AuthenticationStatus AuthenticationStatus;
};
