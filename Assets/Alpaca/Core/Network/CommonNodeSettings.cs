using System.Collections.Generic;

using UnityEngine;

using Alpaca.Serialization;


namespace Alpaca
{

// Size of hashed data (usually a string).
// Note that it might end up packed down smaller when sent over the network.
public enum HashSize : int
{
	  TwoBytes = 0
	, FourBytes
	, EightBytes
	, COUNT
}


[System.Serializable]
public class Channel
{
	public string name;
	public ChannelType type;
}


public class CommonNodeSettings : MonoBehaviour
{
	public TransportType transport = TransportType.UNET;

	public List<Channel> channel = new List<Channel>();
	public List<Entity> entity = new List<Entity>();

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


	[Header("Cryptography")]
	public bool enableEncryption = false;
	/// Whether or not to enable signed diffie hellman key exchange.
	public bool signKeyExchange = false;
	// Pfx file in base64 encoding containing private and public key
	[TextArea]
	public string serverBase64PfxCertificate;



	public ulong ComputeHash()
	{
		channel.Sort( CompareChannelByName );

		using( PooledBitStream stream = PooledBitStream.Get() )
		using( PooledBitWriter writer = PooledBitWriter.Get(stream) )
		{
			writer.WriteString(AlpacaConstant.ALPACA_PROTOCOL_VERSION);
			writer.WriteInt32Packed( (int)transport );

			writer.WriteInt32Packed( channel.Count );
			foreach( Channel c in channel )
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

	private static int CompareChannelByName( Channel a, Channel b )
	{
		// our implementation should not have nulls
		Debug.Assert( a != null && b != null );
		return a.name.CompareTo( b.name );
	}
}

} // namespace Alpaca