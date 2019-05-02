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
		, Authenticated = 1 << 0
		, Encrypted     = 1 << 1
	}

	// NOTE: The size of this enum is limited to 64 values so that the highest two bits are zero.
	// See CustomNode.WrapMessage and CustomNode.UnwrapMessage
	public enum InternalMessage : byte
	{ 
	  // These messages can only be sent to/from prospective (not yet connected) clients
	  ConnectionRequest     // sent from a prospective client to the server
	, ConnectionChallenge   // if using encryption, sent from the server to challenge a prospective client
	, ConnectionResponse    // if using encryption, sent from a prospective client to the server 
	, ConnectionApproved    // sent from the server to prospective client, indicating that it is connected

	  // These messages can only be sent to/from already connected clients
	, ConnectionDisconnect  // sent from a client to the server to disconnect
	, SiblingConnected      // sent from the server to other clients that there is a new client
	, SiblingDisconnected   // sent from the server to other clients that a client disconnected or timed out
	, EntityCreate          // sent from the server to all clients that 1 or more entities were created
	, EntityDestroy         // sent from the server to all clients that 1 or more entities were destroyed
	, ConductRequest        // sent from the client to the server to request ownership of a conduct
	, ConductRelease        // sent from the client to the server to release ownership of a conduct
	, ConductChange         // sent from the server to all clients that 1 or more conducts changed owners
	, TimeSync              // sent from the server to a client to sync time
	, SyncVarServer         // sent from a client that owns a SyncVar to the server to notify a change
	, SyncVarClient         // sent from the server to all non-owner clients to notify a change
	, CustomServer          // custom message sent from client to server
	, CustomClient          // custom message sent from server to client
	, INVALID
	, COUNT = INVALID
	}

	public const int InternalMessageCount = (int)InternalMessage.COUNT;
	public const int InternalMessageMask = 0xFF >> 2;
	
	public static readonly string[] INTERNAL_MESSAGE_NAME = 
	{ "MESSAGE_CERTIFICATE_HAIL"
	, "MESSAGE_CERTIFICATE_HAIL_RESPONSE"
	, "MESSAGE_GREETINGS"
	, "MESSAGE_CONNECTION_REQUEST"
	, "MESSAGE_CONNECTION_APPROVED"
	, "MESSAGE_CLIENT_CONNECT"
	, "MESSAGE_CLIENT_DISCONNECT"
	, "MESSAGE_ADD_ENTITY"
	, "MESSAGE_ADD_ENTITY_ARRAY"
	, "MESSAGE_DESTROY_ENTITY"
	, "MESSAGE_CHANGE_OWNER"
	, "MESSAGE_TIME_SYNC"
	, "MESSAGE_SYNC_VAR_DELTA"
	, "MESSAGE_SYNC_VAR_UPDATE"
	, "MESSAGE_CUSTOM"
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
	{ "CHANNEL_RELIABLE"
	, "CHANNEL_UNRELIABLE"
	, "CHANNEL_CLIENT_RELIABLE"
	};

	public static readonly QosType[] INTERNAL_CHANNEL_TYPE =
	{ QosType.ReliableFragmentedSequenced
	, QosType.Unreliable
	, QosType.Reliable
	};

	public static readonly MessageSecurity[] INTERNAL_CHANNEL_SECURITY =
	{ MessageSecurity.None
	, MessageSecurity.None
	, MessageSecurity.None
	};

	public static MessageSecurity GetSecurity( ChannelIndex index ) { return INTERNAL_CHANNEL_SECURITY[(int)index.GetIndex()]; }
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
		return ((int)_indexPlusOne) - 1;
	}
	public bool IsServer() { return _indexPlusOne == SERVER_SENTINEL_VALUE; }
	public bool IsValidClientIndex() { return _indexPlusOne > 0 && !IsServer(); }

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