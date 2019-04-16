using System.Diagnostics;
using Flags = System.FlagsAttribute;
using UInt32 = System.UInt32;

using UnityEngine.Networking;

using Alpaca.Serialization;


namespace Alpaca
{

public static class AlpacaConstant
{
	public static readonly int CLIENT_OWNER_LIMIT = 32;

	public static readonly string ALPACA_PROTOCOL_VERSION = "0.8.0";

	// Size of hashed data (usually a string).
	// Note that it might end up packed down smaller when sent over the network.
	public enum HashSize : int
	{
		TwoBytes = 0
		, FourBytes
		, EightBytes
		, COUNT
	}

	// The security on a message
	[Flags]
	public enum MessageSecurity
	{
		  None = 0
		, Encrypted     = 1 << 0
		, Authenticated = 1 << 1
	}

	// NOTE: The size of this enum is limited to 64 values so that the highest two bits are zero.
	// See CustomNode.WrapMessage and CustomNode.UnwrapMessage
	public enum InternalMessage : byte
	{ CryptoHail
	, CryptoHailResponse
	, CryptoAccept
	, ConnectionRequest
	, ConnectionApproved
	, ClientDisconnect
	, AddEntity
	, AddEntityArray
	, DestroyEntity
	, ChangeOwner
	, TimeSync
	, SyncVarDelta
	, SyncVarUpdate
	, Custom
	, INVALID
	, COUNT = INVALID
	}

	public const int InternalMessageCount = (int)InternalMessage.COUNT;
	
	public static readonly string[] INTERNAL_MESSAGE_NAME = 
	{ "INTERNAL_MESSAGE_CERTIFICATE_HAIL"
	, "INTERNAL_MESSAGE_CERTIFICATE_HAIL_RESPONSE"
	, "INTERNAL_MESSAGE_GREETINGS"
	, "INTERNAL_MESSAGE_CONNECTION_REQUEST"
	, "INTERNAL_MESSAGE_CONNECTION_APPROVED"
	, "INTERNAL_MESSAGE_CLIENT_DISCONNECT"
	, "INTERNAL_MESSAGE_ADD_ENTITY"
	, "INTERNAL_MESSAGE_ADD_ENTITY_ARRAY"
	, "INTERNAL_MESSAGE_DESTROY_ENTITY"
	, "INTERNAL_MESSAGE_CHANGE_OWNER"
	, "INTERNAL_MESSAGE_TIME_SYNC"
	, "INTERNAL_MESSAGE_SYNC_VAR_DELTA"
	, "INTERNAL_MESSAGE_SYNC_VAR_UPDATE"
	, "INTERNAL_MESSAGE_CUSTOM"
	};

	public static string GetName( InternalMessage message )
	{
		return INTERNAL_MESSAGE_NAME[(int)message];
	}

	public enum InternalChannel
	{
		// for internal messages needing reliable delivery
		// example: connection/disconnection/add object/delete object
			Reliable
		// for internal messages that don't need reliable delivery
		// example: time synchronization
		, Unreliable
		// default for client messages if no custom channels used. Reliable.
		, ClientReliable
		, COUNT
	}

	public const int InternalChannelCount = (int)InternalChannel.COUNT;

	public static readonly string[] INTERNAL_CHANNEL_NAME =
	{ "INTERNAL_CHANNEL_RELIABLE"
	, "INTERNAL_CHANNEL_UNRELIABLE"
	, "INTERNAL_CHANNEL_CLIENT_RELIABLE"
	};

	public static readonly QosType[] INTERNAL_CHANNEL_TYPE =
	{ QosType.ReliableFragmentedSequenced
	, QosType.Reliable
	, QosType.Unreliable
	};
}

public class EntitySet : ArraySet< EntityIndex, Entity >
{
	public EntitySet( int capacity ) : base( capacity )
	{}
}

// merges client custom channel indices by placing them after internal channel indices
// this is never directly serialized, it is passed as a channelId at the UNET level
public struct ChannelIndex : IBitSerializable
{
	// this odd construction is so that a default constructed ChannelIndex will return false from IsValid()
	private UInt32 _indexPlusOne;

	// use static creation functions
	private ChannelIndex( UInt32 index ) { _indexPlusOne = index + 1; }
	
	public int GetIndex()
	{
		Debug.Assert( IsValid() );
		return ((int)_indexPlusOne) - 1;
	}
	public bool IsValid() { return _indexPlusOne > 0; }

	static public ChannelIndex CreateInternal( AlpacaConstant.InternalChannel internalChannel )
	{
		return new ChannelIndex( (UInt32)internalChannel );
	}

	static public ChannelIndex CreateCustom( UInt32 customChannelIndex )
	{
		return new ChannelIndex( customChannelIndex + AlpacaConstant.InternalChannelCount );
	}

	public void Read ( BitReader reader ) { _indexPlusOne = reader.Packed<UInt32>(); }
	public void Write( BitWriter writer ) { writer.Packed<UInt32>( _indexPlusOne ); }
}

public struct NodeIndex : IBitSerializable
{
	public static readonly UInt32 SERVER_SENTINEL_VALUE = 0xFFFFFFFF;
	public static readonly NodeIndex SERVER_NODE_INDEX = new NodeIndex(SERVER_SENTINEL_VALUE);

	// this odd construction is so that a default constructed NodeIndex will return false from IsValidClientIndex()
	private UInt32 _indexPlusOne;

	public NodeIndex( UInt32 index ) { _indexPlusOne = index + 1; }

	public int GetClientIndex()
	{
		Debug.Assert( IsValidClientIndex() );
		Debug.Assert( !IsServer() );
		return ((int)_indexPlusOne) - 1;
	}
	public bool IsServer() { return _indexPlusOne == SERVER_SENTINEL_VALUE; }
	public bool IsValidClientIndex() { return _indexPlusOne > 0; }

	public void Read ( BitReader reader ) { _indexPlusOne = reader.Packed<UInt32>(); }
	public void Write( BitWriter writer ) { writer.Packed<UInt32>( _indexPlusOne ); }
}

public struct EntityPrefabIndex : IBitSerializable
{
	// this odd construction is so that a default constructed index will return false from IsValid()
	private UInt32 _indexPlusOne;

	public EntityPrefabIndex( UInt32 index ) { _indexPlusOne = index + 1; }

	public int GetIndex()
	{
		Debug.Assert( IsValid() );
		return ((int)_indexPlusOne) - 1;
	}
	public bool IsValid() { return _indexPlusOne > 0; }

	public void Read ( BitReader reader ) { _indexPlusOne = reader.Packed<UInt32>(); }
	public void Write( BitWriter writer ) { writer.Packed<UInt32>( _indexPlusOne ); }
} 

// EntityIndexes should be kept unique across the network.
// This is usually accomplished by only having the ServerNode make new Entities.
// The server can then keep a trivial count of entity ids that have been handed out.
public struct EntityIndex : IBitSerializable
{
	// this odd construction is so that a default constructed index will return false from IsValid()
	private UInt32 _indexPlusOne;

	public EntityIndex( UInt32 index ) { _indexPlusOne = index + 1; }

	public UInt32 GetIndex()
	{
		Debug.Assert( IsValid() );
		return _indexPlusOne - 1;
	}
	public bool IsValid() { return _indexPlusOne > 0; }

	public void Read ( BitReader reader ) { _indexPlusOne = reader.Packed<UInt32>(); }
	public void Write( BitWriter writer ) { writer.Packed<UInt32>( _indexPlusOne ); }
}


} // namespace Alpaca