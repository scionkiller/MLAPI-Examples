// used for custom Alpaca messages
enum CustomMessageType : byte
{
	  Invalid = 0
	, RoomNameRequest    // Client to Server
	, RoomNameResponse   // Server to Client
	, SpawnAvatarRequest // Client to Server
}
