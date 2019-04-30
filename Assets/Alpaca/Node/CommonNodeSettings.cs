﻿using UInt32 = System.UInt32;
using Int32 = System.Int32;
using UInt64 = System.UInt64;
using Serializable = System.SerializableAttribute;

using UnityEngine;
using UnityEngine.Networking;

using Alpaca.Serialization;
using InternalMessage = Alpaca.AlpacaConstant.InternalMessage;
using InternalChannel = Alpaca.AlpacaConstant.InternalChannel;
using HashSize = Alpaca.AlpacaConstant.HashSize;


namespace Alpaca
{

	// returned by PollReceive
	public enum ReceiveEvent
	{
		  Error
		, Message
		, Connect
		, Disconnect
		, NoMoreEvents
		, IgnoreThisEvent
	}



[Serializable]
public class CustomChannelSettings
{
	public string name;
	public QosType type;
}

// POD class, read-only
public class Channel
{
	ChannelIndex _index;
	string _name;
	QosType _type;

	public ChannelIndex GetIndex() { return _index; }
	public string GetName() { return _name; }
	public QosType GetQosType() { return _type; }

	public Channel( ChannelIndex index, string name, QosType type )
	{
		_index = index;
		_name = name;
		_type = type;
	}
}


public class CommonNodeSettings : MonoBehaviour
{
	public CustomChannelSettings[] customChannel = null;
	public Entity[] entity = null;

	// The size of the receive message buffer. This is the max message size including any library overhead
	public ushort messageBufferSize = 1024;

	// The max amount of events to process per send or receive tick.
	// This is to prevent flooding. 0 means no limit.
	// At runtime, use _maxEventCount on CommonNode() instead of accessing this directly.
	public uint maxEventCount = 500;

	// in seconds
	// TODO: cozeroff not currently used
	public int lagCompensationHistory = 5;

	// How many bytes to use for hashing strings. Leave this to 2 bytes unless you are facing hash collisions
	public HashSize stringHashSize = HashSize.TwoBytes;

	// If your logic uses the NetworkedTime, this should probably be turned off. If however it's needed to maximize accuracy, this is recommended to be turned on.
	// TODO: cozeroff not currently used
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
}

// ignore Obsolete warning for uNet
#pragma warning disable 618

public abstract class CommonNode
{
	static readonly int BIT_POOL_CAPACITY = 32;

	protected CommonNodeSettings _commonSettings = null;

	// A synchronized time, represents the time in seconds since the server started. Replicated across all nodes.
	protected float _networkTime;

	// identifies this Node across the network, unique for every Node.
	protected NodeIndex _localIndex;

	FixedPool<BitReader> _readerPool;
	FixedPool<BitWriter> _writerPool;

	// for each channel, the id of the channel matches it's index into this table
	// this is initialized in InitializeNetwork
	protected Channel[] _channel = null;

	// this is initialized in InitializeNetwork
	protected uint _maxEventCount;

	System.Action<Entity> _onAvatarSpawn = null;
	System.Action<Entity> _onEntitySpawn = null;
	System.Action<NodeIndex, BitReader> _onCustomMessage = null;


	public float GetNetworkTime() { return _networkTime; }
	public NodeIndex GetLocalNodeIndex() { return _localIndex; }
	public int GetConnectionPort() { return _commonSettings.connectionPort; }
	public int GetMaxMessageLength() { return _commonSettings.messageBufferSize; }
	public BitReader GetPooledReader() { return _readerPool.Get(); }
	public BitWriter GetPooledWriter() { return _writerPool.Get(); }

	public void SetOnAvatarSpawn     ( System.Action<Entity> callback               ) { _onAvatarSpawn      = callback; }
	public void SetOnEntitySpawn     ( System.Action<Entity> callback               ) { _onEntitySpawn      = callback; }
	public void SetOnCustomMessage   ( System.Action<NodeIndex, BitReader> callback ) { _onCustomMessage    = callback; }

	public abstract bool Start( out string error );

	// PROTECTED

	protected CommonNode( CommonNodeSettings commonSettings )
	{
		_commonSettings = commonSettings;

		// network time and _localIndex must be setup by child classes
		// these are just default invalid values.
		_networkTime = -1f;
		_localIndex = new NodeIndex();

		int bufferLength = GetMaxMessageLength();
		BitReader protoReader = new BitReader( new DataStream( bufferLength ) );
		BitWriter protoWriter = new BitWriter( new DataStream( bufferLength ) );
		_readerPool = new FixedPool<BitReader>( protoReader, BIT_POOL_CAPACITY );
		_writerPool = new FixedPool<BitWriter>( protoWriter, BIT_POOL_CAPACITY );
	}

	// Initializes the network transport and builds the _channel array.
	// Returns null on failure, and sets error string with description.
	protected ConnectionConfig InitializeNetwork( out string error )
	{
		_maxEventCount = _commonSettings.maxEventCount > 0 ? _commonSettings.maxEventCount : UInt32.MaxValue;

		GlobalConfig gConfig = new GlobalConfig(); // default settings
		gConfig.MaxPacketSize = _commonSettings.messageBufferSize;
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

			_channel[i] = new Channel( ChannelIndex.CreateInternal( (InternalChannel)i ), name, qos );
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

			_channel[offsetIndex] = new Channel( ChannelIndex.CreateCustom( (UInt32)i ), c.name, c.type );
		}

		error = "";
		return config;
	}

	// polls the underlying UNET transport for packets. Note that we return the connectionId without
	// converting it into a NodeIndex because the interpretation depends on whether or not the node
	// executing this function is a ServerNode or a ClientNode
	protected ReceiveEvent PollReceive( DataStream stream, out int connectionId, out ChannelIndex channel )
	{
		int hostId;    // this will always be the same port, we don't care about this
		int channelId; // corresponds to ChannelIndex
		int receivedSize;
		byte errorByte;

		NetworkEventType unetEvent = NetworkTransport.Receive( out hostId, out connectionId, out channelId, stream.GetBuffer(), stream.GetByteCapacity(), out receivedSize, out errorByte );
		stream.SetByteLength( receivedSize );

		NetworkError unetError = (NetworkError)errorByte;
		if(  unetError != NetworkError.Ok
		  && unetError != NetworkError.Timeout
		  )
		{
			// polling failed
			Log.Error( StringFromError( errorByte ) );
			channel = new ChannelIndex();
			return ReceiveEvent.Error;
		}

		channel = _channel[channelId].GetIndex();

		// translate UNET NetworkEventType to EventType
		if( unetError == NetworkError.Timeout )
		{
			return ReceiveEvent.Disconnect;
		}
		else
		{
			switch( unetEvent )
			{
				case NetworkEventType.DataEvent:
					return ReceiveEvent.Message;
				case NetworkEventType.ConnectEvent:
					return ReceiveEvent.Connect;
				case NetworkEventType.DisconnectEvent:
					return ReceiveEvent.Disconnect;
				case NetworkEventType.Nothing:				
					return ReceiveEvent.NoMoreEvents;
				case NetworkEventType.BroadcastEvent:
				default:
					Log.Error( "Received Broadcast or unknown message type from UNET transport layer" );
					return ReceiveEvent.Error;
			}
		}
	}

	// Extracts body of message, which could include decrypting and/or authentication.
	protected InternalMessage UnwrapMessage( BitReader reader, NodeIndex client )
	{
		int inputLength = reader.GetLength();
		Log.Info( $"Unwrapping incoming message from {client} : {inputLength} bytes" );

		if( inputLength < 1 )
		{
			Log.Error( $"The incoming message from {client} was too small" );
			return InternalMessage.INVALID;
		}

		// the first byte of the wrapped message is either:
		// 1. a byte indicating the message type, which always has two leading zeros (see InternalMessage)
		// 2. two bits indicating encryption and authentication, where at least one bit is on, followed by 6 discarded bits

		bool isEncrypted     = reader.Normal<bool>();
		bool isAuthenticated = reader.Normal<bool>();
				
		if( !isEncrypted && !isAuthenticated )
		{
			// no encryption or authentication case, reset read head so we can read byte of message type
			DataStream s = reader.GetStream();
			s.SetBitPosition( s.GetBitPosition() - 2 );
		}
		else
		{
			// encryption case, align the read head (ignoring the next 6 bits)
			reader.AlignToByte();

			if( !_commonSettings.enableEncryption )
			{
				Log.Error( "Got a encrypted and/or authenticated message but encryption was not enabled" );
				return InternalMessage.INVALID;
			}

			if( isAuthenticated )
			{
				if( !CheckAuthentication( reader, client ) )
				{
					return InternalMessage.INVALID;
				}
			}

			if( isEncrypted )
			{
				if( !PerformDecryption( reader, client ) )
				{
					return InternalMessage.INVALID;
				}
			}
		}

		// At this point, the reader should be in a state where the next byte
		// is the InternalMessage type, and the message body follows it

		byte messageByte = reader.Normal<byte>();
		if( messageByte >= (byte)InternalMessage.COUNT )
		{
			Log.Error( $"Unknown InternalMessage type {messageByte} encountered while unwrapping message." );
			return InternalMessage.INVALID;
		}
		
		return (InternalMessage)messageByte;
	}

	protected bool WrapMessage( InternalMessage message, BitWriter writer, byte[] publicKey )
	{
		try
		{
			bool encrypted = ((flags & SecuritySendFlags.Encrypted) == SecuritySendFlags.Encrypted) && AlpacaNetwork.GetSingleton().config.EnableEncryption;
			bool authenticated = (flags & SecuritySendFlags.Authenticated) == SecuritySendFlags.Authenticated && AlpacaNetwork.GetSingleton().config.EnableEncryption;

			PooledBitStream outStream = PooledBitStream.Get();

			using (PooledBitWriter outWriter = PooledBitWriter.Get(outStream))
			{
				outWriter.WriteBit(encrypted);
				outWriter.WriteBit(authenticated);
				
				if (authenticated || encrypted)
				{
					AlpacaNetwork network = AlpacaNetwork.GetSingleton();

					outWriter.WritePadBits();
					long hmacWritePos = outStream.Position;

					if (authenticated) outStream.Write(HMAC_PLACEHOLDER, 0, HMAC_PLACEHOLDER.Length);

					if (encrypted)
					{
						using (RijndaelManaged rijndael = new RijndaelManaged())
						{
							rijndael.GenerateIV();
							rijndael.Padding = PaddingMode.PKCS7;

							byte[] key = network.GetPublicEncryptionKey(clientId);
							if (key == null)
							{
								Log.Error("Failed to grab key");
								return null;
							}

							rijndael.Key = key;

							outStream.Write(rijndael.IV);

							using (CryptoStream encryptionStream = new CryptoStream(outStream, rijndael.CreateEncryptor(), CryptoStreamMode.Write))
							{
								encryptionStream.WriteByte(messageType);
								encryptionStream.Write(messageBody.GetBuffer(), 0, (int)messageBody.Length);
							}
						}
					}
					else
					{
						outStream.WriteByte(messageType);
						outStream.Write(messageBody.GetBuffer(), 0, (int)messageBody.Length);
					}

					if (authenticated)
					{
						byte[] key = network.GetPublicEncryptionKey(clientId);
						if (key == null)
						{
							Log.Error("Failed to grab key");
							return null;
						}

						using (HMACSHA256 hmac = new HMACSHA256(key))
						{
							byte[] computedHmac = hmac.ComputeHash(outStream.GetBuffer(), 0, (int)outStream.Length);

							outStream.Position = hmacWritePos;
							outStream.Write(computedHmac, 0, computedHmac.Length);
						}
					}
				}
				else
				{
					outWriter.WriteBits(messageType, 6);
					outStream.Write(messageBody.GetBuffer(), 0, (int)messageBody.Length);
				}
			}

			return outStream;
		}
		catch (Exception e)
		{
			Log.Error("Error while wrapping headers");
			Log.Error(e.ToString());

			return null;
		}
	}

	protected UInt64 ComputeSettingsHash()
	{
		using( BitWriter writer = GetPooledWriter() )
		{
			// TODO: cozeroff
			//writer.WriteString(AlpacaConstant.ALPACA_PROTOCOL_VERSION);

			writer.Packed<Int32>( _commonSettings.customChannel.Length );
			foreach( CustomChannelSettings c in _commonSettings.customChannel )
			{
				// TODO: cozeroff
				//writer.WriteString( c.name );
				writer.Packed<Int32>( (Int32)c.type );
			}

			// we do not write the Entity's here because it is allowed for the client and server to have different prefabs.
			// eventually, we should do a more advanced check, since the prefabs do need to have matching Conducts.
			writer.Packed<Int32>( _commonSettings.messageBufferSize );
			writer.Packed<UInt32>( _commonSettings.maxEventCount );
			writer.Packed<Int32>( _commonSettings.lagCompensationHistory );
			writer.Packed<Int32>( (Int32)_commonSettings.stringHashSize );
			writer.Packed<Int32>( (Int32)_commonSettings.logLevel );
			writer.Packed<bool>( _commonSettings.enableTimeResync );
			writer.Packed<bool>( _commonSettings.enableEncryption );
			writer.Packed<bool>( _commonSettings.signKeyExchange );
			writer.AlignToByte();

			return writer.GetStableHash64();
		}
	}

	protected void Send( int connectionId, byte[] publicKey, InternalMessage message, ChannelIndex channel, BitWriter writer, bool sendImmediately )
	{
		if( WrapMessage( message, writer, publicKey ) )
		{
			//_profiler.StartEvent(TickType.Send, (uint)stream.Length, channelName, AlpacaConstant.INTERNAL_MESSAGE_NAME[messageType]);
			byte errorAsByte;
			byte[] stream = writer.GetStream();
			if( sendImmediately )
			{
				NetworkTransport.Send(netId.HostId, netId.ConnectionId, channelId, dataBuffer, dataSize, out error);
			}
			else
			{
				NetworkTransport.QueueMessageForSending(netId.HostId, netId.ConnectionId, channelId, dataBuffer, dataSize, out error);
			}

			QueueMessageForSending( connectionId, stream.GetBuffer(), stream.GetByteLength(), AlpacaConstant.GetName(message), sendImmediately, out errorAsByte);
			//_profiler.EndEvent();

			if( (NetworkError)errorAsByte != NetworkError.Ok )
			{
				Log.Error( $"Sending message failed with error{StringFromError(errorAsByte)}" );
			}
		}
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


	// PRIVATE

	/*
	internal static readonly Dictionary<string, int> channels = new Dictionary<string, int>();
	internal static readonly Dictionary<int, string> reverseChannels = new Dictionary<int, string>();

	private static readonly byte[] IV_BUFFER = new byte[16];
	private static readonly byte[] HMAC_BUFFER = new byte[32];
	private static readonly byte[] HMAC_PLACEHOLDER = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
	*/

	bool CheckAuthentication( BitReader reader, NodeIndex client )
	{
		/*
		long hmacStartPos = inputStream.Position;
		int readHmacLength = inputStream.Read(HMAC_BUFFER, 0, HMAC_BUFFER.Length);
		if (readHmacLength != HMAC_BUFFER.Length)
		{
			Log.Error("HMAC length was invalid");
			return false;
		}

		// Now we have read the HMAC, we need to set the hmac in the input to 0s to perform the HMAC.
		inputStream.Position = hmacStartPos;
		inputStream.Write(HMAC_PLACEHOLDER, 0, HMAC_PLACEHOLDER.Length);

		byte[] key = network.GetPublicEncryptionKey(clientId);
		if( key == null )
		{
			Log.Error("Failed to grab key");
			return false;
		}

		using (HMACSHA256 hmac = new HMACSHA256(key))
		{
			byte[] computedHmac = hmac.ComputeHash(inputStream.GetBuffer(), 0, (int)inputStream.Length);
			if (!CryptographyHelper.ConstTimeArrayEqual(computedHmac, HMAC_BUFFER))
			{
				Log.Error("Received HMAC did not match the computed HMAC");
				return false;
			}
		}
		*/

		return true;
	}

	bool PerformDecryption( BitReader reader, NodeIndex client )
	{
		/*
		int ivRead = inputStream.Read(IV_BUFFER, 0, IV_BUFFER.Length);

		if (ivRead != IV_BUFFER.Length)
		{
			Log.Error("Invalid IV size");
			messageType = AlpacaConstant.INVALID;
			return null;
		}

		PooledBitStream outputStream = PooledBitStream.Get();

		using (RijndaelManaged rijndael = new RijndaelManaged())
		{
			rijndael.IV = IV_BUFFER;
			rijndael.Padding = PaddingMode.PKCS7;

			byte[] key = network.GetPublicEncryptionKey(clientId);
			if (key == null)
			{
				Log.Error("Failed to grab key");
				messageType = AlpacaConstant.INVALID;
				return null;
			}

			rijndael.Key = key;

			using (CryptoStream cryptoStream = new CryptoStream(outputStream, rijndael.CreateDecryptor(), CryptoStreamMode.Write))
			{
				cryptoStream.Write(inputStream.GetBuffer(), (int)inputStream.Position, (int)(inputStream.Length - inputStream.Position));
			}

			outputStream.Position = 0;

			if (outputStream.Length == 0)
			{
				Log.Error("The incomming message was too small");
				messageType = AlpacaConstant.INVALID;
				return null;
			}

			int msgType = outputStream.ReadByte();
			messageType = msgType == -1 ? AlpacaConstant.INVALID : (byte)msgType;
		}

		return outputStream;
		*/

		return true;
	}
}

#pragma warning restore 618

} // namespace Alpaca