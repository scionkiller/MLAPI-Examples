namespace Alpaca
{
	public enum TransportType : int
	{
		  UNET
		, COUNT
	}

    public enum ChannelType : int
    {
        // Unreliable message
        Unreliable,
        // Unreliable with fragmentation support
        UnreliableFragmented,
        // Unreliable with sequencing
        UnreliableSequenced,
        // Reliable message
        Reliable,
        // Reliable with fragmentation support
        ReliableFragmented,
        // Reliable message where messages are guaranteed to be in the right order
        ReliableSequenced,
        // A unreliable state update message
        StateUpdate,
        // A reliable state update message
        ReliableStateUpdate,
        // A reliable message with high priority
        AllCostDelivery,
        // Unreliable message with fragmentation where older messages are dropped
        UnreliableFragmentedSequenced,
        // A reliable message with guaranteed order with fragmentation support
        ReliableFragmentedSequenced
    }

    public static class AlpacaConstant
    {
		public static readonly int CLIENT_OWNER_LIMIT = 32;
		public static readonly int PREFAB_INDEX_INVALID = -1;

        public static readonly string ALPACA_PROTOCOL_VERSION = "0.8.0";

		// must be const, used in switch statement
        public const byte ALPACA_CERTIFICATE_HAIL              =  0;
        public const byte ALPACA_CERTIFICATE_HAIL_RESPONSE     =  1;
        public const byte ALPACA_GREETINGS                     =  2;
        public const byte ALPACA_CONNECTION_REQUEST            =  3;
        public const byte ALPACA_CONNECTION_APPROVED           =  4;
		public const byte ALPACA_CLIENT_DISCONNECT             =  5;
        public const byte ALPACA_ADD_OBJECT                    =  6;
		public const byte ALPACA_ADD_OBJECTS                   =  7;
        public const byte ALPACA_DESTROY_OBJECT                =  8;
        public const byte ALPACA_CHANGE_OWNER                  =  9;
        public const byte ALPACA_TIME_SYNC                     = 10;
        public const byte ALPACA_NETWORKED_VAR_DELTA           = 11;
        public const byte ALPACA_NETWORKED_VAR_UPDATE          = 12;
        public const byte ALPACA_SERVER_RPC                    = 13;
        public const byte ALPACA_SERVER_RPC_REQUEST            = 14;
        public const byte ALPACA_SERVER_RPC_RESPONSE           = 15;
        public const byte ALPACA_CLIENT_RPC                    = 16;
        public const byte ALPACA_CLIENT_RPC_REQUEST            = 17;
        public const byte ALPACA_CLIENT_RPC_RESPONSE           = 18;
        public const byte ALPACA_CUSTOM_MESSAGE                = 19;
        public const byte INVALID                              = 32;
        
        public static readonly string[] MESSAGE_NAMES = 
		{ "ALPACA_CERTIFICATE_HAIL"
		, "ALPACA_CERTIFICATE_HAIL_RESPONSE"
		, "ALPACA_GREETINGS"
		, "ALPACA_CONNECTION_REQUEST"
		, "ALPACA_CONNECTION_APPROVED"
		, "ALPACA_CLIENT_DISCONNECT"
		, "ALPACA_ADD_OBJECT"
		, "ALPACA_ADD_OBJECTS"
		, "ALPACA_DESTROY_OBJECT"
		, "ALPACA_CHANGE_OWNER"
		, "ALPACA_TIME_SYNC"
		, "ALPACA_NETWORKED_VAR_DELTA"
		, "ALPACA_NETWORKED_VAR_UPDATE"
		, "ALPACA_SERVER_RPC"
		, "ALPACA_SERVER_RPC_REQUEST"
		, "ALPACA_SERVER_RPC_RESPONSE"
		, "ALPACA_CLIENT_RPC"
		, "ALPACA_CLIENT_RPC_REQUEST"
		, "ALPACA_CLIENT_RPC_RESPONSE"
		, "ALPACA_CUSTOM_MESSAGE"
		, ""
		, ""
		, ""
		, ""
		, ""
		, ""
		, ""
		, ""
		, ""
		, ""
		, ""
		, "INVALID" // 32
        };
    }
}