using UInt32 = System.UInt32;
using Serializable = System.SerializableAttribute;

using UnityEngine;
using UnityEngine.Networking;

using Alpaca.Serialization;
using InternalChannel = Alpaca.AlpacaConstant.InternalChannel;
using HashSize = Alpaca.AlpacaConstant.HashSize;


namespace Alpaca
{

[Serializable]
public class CustomChannelSettings
{
	public string name;
	public QosType type;
}

// POD class, read-only
public class Channel
{
	int _id;
	string _name;
	QosType _type;

	public int GetId() { return _id; }
	public string GetName() { return _name; }
	public QosType GetQosType() { return _type; }

	public Channel( int id, string name, QosType type )
	{
		_id = id;
		_name = name;
		_type = type;
	}
}


public class CommonNodeSettings : MonoBehaviour
{
	public CustomChannelSettings[] customChannel = null;
	public Entity[] entity = null;

	// The size of the receive message buffer. This is the max message size including any library overhead
	public int messageBufferSize = 1024;

	// Amount of times per second every pending message will be sent and received message queue will be emptied
	// Having Conduct send rates greater than this is pointless.
	public int networkChecksPerSecond = 64;
	// The max amount of messages to process per send or receive tick. This is to prevent flooding.
	public int maxEventsPerCheck = 500;

	public int maxClientCount = 64;

	// in seconds
	public int lagCompensationHistory = 5;

	// How many bytes to use for hashing strings. Leave this to 2 bytes unless you are facing hash collisions
	public HashSize stringHashSize = HashSize.TwoBytes;

	// If your logic uses the NetworkedTime, this should probably be turned off. If however it's needed to maximize accuracy, this is recommended to be turned on
	public bool enableTimeResync = false;

	public LogLevel logLevel = LogLevel.Warning;

	// the port that the server will open to listen for connections, and that the client will attempt to connect to
	public int connectionPort = 7777;


	// TODO: cozeroff figure this out again
	[Header("Cryptography")]
	public bool enableEncryption = false;
	/// Whether or not to enable signed diffie hellman key exchange.
	public bool signKeyExchange = false;
	// Pfx file in base64 encoding containing private and public key
	[TextArea]
	public string serverBase64PfxCertificate;



	public ulong ComputeHash()
	{
		using( PooledBitStream stream = PooledBitStream.Get() )
		using( PooledBitWriter writer = PooledBitWriter.Get(stream) )
		{
			writer.WriteString(AlpacaConstant.ALPACA_PROTOCOL_VERSION);

			writer.WriteInt32Packed( customChannel.Length );
			foreach( CustomChannelSettings c in customChannel )
			{
				writer.WriteString( c.name );
				writer.WriteInt32Packed( (int)c.type );
			}

			// we do not write the Entity's here because it is allowed for the client and server to have different prefabs.
			// eventually, we should do a more advanced check, since the prefabs do need to have matching Conducts.
			writer.WriteInt32Packed( messageBufferSize );
			writer.WriteInt32Packed( networkChecksPerSecond );
			writer.WriteInt32Packed( maxEventsPerCheck );
			writer.WriteInt32Packed( maxClientCount );
			writer.WriteInt32Packed( lagCompensationHistory );
			writer.WriteInt32Packed( (int)stringHashSize );
			writer.WriteBool( enableTimeResync );
			writer.WriteInt32Packed( (int)logLevel );

			writer.WriteBool( enableEncryption );
			writer.WriteBool( signKeyExchange );

			return stream.ToArray().GetStableHash64();
		}
	}
}

// ignore Obsolete warning for uNet
#pragma warning disable 618

public abstract class CommonNode
{
	// returned by PollReceive
	protected enum ReceiveEvent
	{
		// new message
		Message
		// A client is connected, or client connected to server
		, Connect
		// A client disconnected, or client disconnected from server
		, Disconnect
		// No new event, indicates polling can stop for this frame
		, Nothing
	}

	protected CommonNodeSettings _commonSettings = null;

	// storage for a single message of max size
	protected byte[] _messageBuffer;

	// A synchronized time, represents the time in seconds since the server started. Replicated across all nodes.
	protected float _networkTime;

	// identifies this Node across the network, unique for every Node.
	protected NodeIndex _localIndex;

	// for each channel, the id of the channel matches it's index into this table
	// this is initialized in InitializeNetwork
	protected Channel[] _channel = null;

	System.Action<Entity> _onAvatarSpawn = null;
	System.Action<Entity> _onEntitySpawn = null;
	System.Action<NodeIndex, BitReader> _onCustomMessage = null;


	public float GetNetworkTime() { return _networkTime; }
	public NodeIndex GetLocalNodeIndex() { return _localIndex; }
	public int GetConnectionPort() { return _commonSettings.connectionPort; }

	public void SetOnAvatarSpawn     ( System.Action<Entity> callback               ) { _onAvatarSpawn      = callback; }
	public void SetOnEntitySpawn     ( System.Action<Entity> callback               ) { _onEntitySpawn      = callback; }
	public void SetOnCustomMessage   ( System.Action<NodeIndex, BitReader> callback ) { _onCustomMessage    = callback; }

	public abstract bool Start( out string error );


	protected CommonNode( CommonNodeSettings commonSettings )
	{
		_commonSettings = commonSettings;

		_messageBuffer = new byte[_commonSettings.messageBufferSize];

		// network time and _localIndex must be setup by child classes
		// these are just default invalid values.
		_networkTime = -1f;
		_localIndex = new NodeIndex();
	}

	// Initializes the network transport and builds the _channel array.
	// Returns null on failure, and sets error string with description.
	protected ConnectionConfig InitializeNetwork( out string error )
	{
		NetworkTransport.Init();

		ConnectionConfig config = new ConnectionConfig();
		config.SendDelay = 0;

		int channelCount = AlpacaConstant.InternalChannelCount + _commonSettings.customChannel.Length;
		_channel = new Channel[channelCount];

		// add internal channels
		for( int i = 0; i < AlpacaConstant.InternalChannelCount; ++i )
		{
			string name = AlpacaConstant.INTERNAL_CHANNEL_NAME[i];
			QosType qos = AlpacaConstant.INTERNAL_CHANNEL_TYPE[i];

			int index = config.AddChannel( qos );
			if( index != i )
			{
				error = Log.PrefixMessage( string.Format( "Unexpected channel index {0} while attempting to create {1}", index, name ) );
				return null;
			}

			_channel[i] = new Channel( i, name, qos );
		}

		// add custom channels
		for( int i = 0; i < _commonSettings.customChannel.Length; ++i )
		{
			CustomChannelSettings c = _commonSettings.customChannel[i];
			
			int index = config.AddChannel( c.type );
			int offsetIndex = i + AlpacaConstant.InternalChannelCount;
			if( index != offsetIndex )
			{
				error = Log.PrefixMessage( string.Format( "Unexpected channel index {0} while attempting to create {1}", index, c.name ) );
				return null;
			}

			_channel[offsetIndex] = new Channel( offsetIndex, c.name, c.type );
		}

		error = "";
		return config;
	}


	// PROTECTED STATIC

	protected static string StringFromError( byte uNetErrorByte )
	{
		NetworkError error = (NetworkError)uNetErrorByte;
		if( error == NetworkError.Ok )
		{
			return string.Empty;
		}
		else
		{
			return "[uNet] Transport error: " + error.ToString();
		}
	}
}

#pragma warning restore 618

} // namespace Alpaca