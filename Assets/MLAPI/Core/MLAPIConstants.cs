namespace Alpaca.Data
{
    public static class Constants
    {
        public const string ALPACA_PROTOCOL_VERSION = "6.0.0";

        public const byte ALPACA_CERTIFICATE_HAIL              =  0;
        public const byte ALPACA_CERTIFICATE_HAIL_RESPONSE     =  1;
        public const byte ALPACA_GREETINGS                     =  2;
        public const byte ALPACA_CONNECTION_REQUEST            =  3;
        public const byte ALPACA_CONNECTION_APPROVED           =  4;
        public const byte ALPACA_ADD_OBJECT                    =  5;
        public const byte ALPACA_CLIENT_DISCONNECT             =  6;
        public const byte ALPACA_DESTROY_OBJECT                =  7;
        public const byte ALPACA_SPAWN_POOL_OBJECT             =  8;
        public const byte ALPACA_DESTROY_POOL_OBJECT           =  9;
        public const byte ALPACA_CHANGE_OWNER                  = 10;
        public const byte ALPACA_ADD_OBJECTS                   = 11;
        public const byte ALPACA_TIME_SYNC                     = 12;
        public const byte ALPACA_NETWORKED_VAR_DELTA           = 13;
        public const byte ALPACA_NETWORKED_VAR_UPDATE          = 14;
        public const byte ALPACA_SERVER_RPC                    = 15;
        public const byte ALPACA_SERVER_RPC_REQUEST            = 16;
        public const byte ALPACA_SERVER_RPC_RESPONSE           = 17;
        public const byte ALPACA_CLIENT_RPC                    = 18;
        public const byte ALPACA_CLIENT_RPC_REQUEST            = 19;
        public const byte ALPACA_CLIENT_RPC_RESPONSE           = 20;
        public const byte ALPACA_CUSTOM_MESSAGE                = 21;
        public const byte INVALID                             = 32;
        
        public static readonly string[] MESSAGE_NAMES = {
            "ALPACA_CERTIFICATE_HAIL", // 0
            "ALPACA_CERTIFICATE_HAIL_RESPONSE",
            "ALPACA_GREETINGS",
            "ALPACA_CONNECTION_REQUEST",
            "ALPACA_CONNECTION_APPROVED",
            "ALPACA_ADD_OBJECT",
            "ALPACA_CLIENT_DISCONNECT",
            "ALPACA_DESTROY_OBJECT",
            "ALPACA_SPAWN_POOL_OBJECT",
            "ALPACA_DESTROY_POOL_OBJECT",
            "ALPACA_CHANGE_OWNER",
            "ALPACA_ADD_OBJECTS",
            "ALPACA_TIME_SYNC",
            "ALPACA_NETWORKED_VAR_DELTA",
            "ALPACA_NETWORKED_VAR_UPDATE",
            "ALPACA_SERVER_RPC",
            "ALPACA_SERVER_RPC_REQUEST",
            "ALPACA_SERVER_RPC_RESPONSE",
            "ALPACA_CLIENT_RPC",
            "ALPACA_CLIENT_RPC_REQUEST",
            "ALPACA_CLIENT_RPC_RESPONSE",
            "ALPACA_CUSTOM_MESSAGE",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
			"",
            "INVALID" // 32
        };
    }
}