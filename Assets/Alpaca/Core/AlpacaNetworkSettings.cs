﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.IO;
using System.Security.Cryptography.X509Certificates;

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

using Alpaca.Logging;
using Alpaca.Components;
using Alpaca.Configuration;
using Alpaca.Cryptography;
using Alpaca.Data;
using Alpaca.Internal;
using Alpaca.Profiling;
using Alpaca.Serialization;
using Alpaca.Transports;
using BitStream = Alpaca.Serialization.BitStream;


namespace Alpaca
{

// Size of hashed data (usually a string).
// Note that it might end up packed down smaller when sent over the network.
public enum HashSize
{
	  TwoBytes = 0
	, FourBytes
	, EightBytes
	, COUNT
}

public class AlpacaNetworkSettings : MonoBehaviour
{
	const int HASH_SIZE_BITS = 2;

	// Nothing to see here, just making sure we don't screw ourselves over.
	// static assert that we are sending enough bits to encode the hash size
	#pragma warning disable 219 // disable warning that this fake constant isn't used
	const uint _STATIC_ASSERT_HASH_SIZE_BITS_BIG_ENOUGH = ((int)HashSize.COUNT <= (1<<HASH_SIZE_BITS) ) ? 0 : -666;
	#pragma warning restore 219

	static readonly uint protocolVersion = 1;


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


	[Header("Cryptography")]
	public bool enableEncryption = false;
	/// Whether or not to enable signed diffie hellman key exchange.
	public bool signKeyExchange = false;
	// Pfx file in base64 encoding containing private and public key
	[TextArea]
	public string serverBase64PfxCertificate;



	// TODO: cozeroff These are client only settings, and should be on a client settings object
	// when we get around to separating the work of the client from the work of the server

	// BEGIN CLIENT ONLY
	[Header("Client Only")]
	// how to contact the server to connect
	public int connectPort = 7777;
	public string connectAddress = "127.0.0.1";

	// END CLIENT ONLY

	// TODO: cozeroff These are server only settings, and should be on a client settings object
	// when we get around to separating the work of the client from the work of the server

	// BEGIN SERVER ONLY
	[Header("ServerOnly")]
	public int clientHandshakeTimeout = 10;

	// END SERVER ONLY



	public ulong ComputeHash()
	{
		channel.Sort( CompareChannelByName );

		using( PooledBitStream stream = PooledBitStream.Get() )
		using( PooledBitWriter writer = PooledBitWriter.Get(stream) )
		{
			writer.WriteUInt32Packed( protocolVersion );
			writer.WriteString(AlpacaConstant.ALPACA_PROTOCOL_VERSION);

			for (int i = 0; i < Channels.Count; i++)
			{
				writer.WriteString(Channels[i].Name);
				writer.WriteByte((byte)Channels[i].Type);
			}

			writer.WriteBool(EnableEncryption);
			writer.WriteBool(SignKeyExchange);
			writer.WriteBits((byte)StringHashSize, HASH_SIZE_BITS);
			stream.PadStream();

			ConfigHash = stream.ToArray().GetStableHash64();
			return ConfigHash.Value;
		}
	}

	private static int CompareChannelByName( Channel a, Channel b )
	{
		// our implementation should not have nulls
		Debug.Assert( a != null && b != null );
		return a.Name.CompareTo( b.Name );
	}
}



























[AddComponentMenu("Alpaca/AlpacaNetwork", -100)]
public class AlpacaNetwork : MonoBehaviour
{

	/*
	/// <summary>
	/// A synchronized time, represents the time in seconds since the server application started. Is replicated across all clients
	/// </summary>
	public float NetworkTime { get; internal set; }

	[HideInInspector]
	public LogLevel LogLevel = LogLevel.Normal;
	
	/// <summary>
	/// Gets the networkId of the server
	/// </summary>
	public uint ServerClientId => config.NetworkTransport != null ? config.NetworkTransport.ServerClientId : 0;

	/// <summary>
	/// The clientId the server calls the local client by, only valid for clients
	/// </summary>
	public uint LocalClientId 
	{
		get
		{
			if (IsServer) return config.NetworkTransport.ServerClientId;
			else return localClientId;
		}
		internal set
		{
			localClientId = value;
		}
	}
	private uint localClientId;

	// TODO: make these private and improve encapsulation
	// Will require moving of lots of code that belongs in this file but is scattered around
	public ClientSet _connectedClients;
	public PendingClientSet _pendingClients;

	public bool IsServer { get; internal set; }

	public bool IsClient { get; internal set; }

	public bool IsListening { get; internal set; }
	private byte[] messageBuffer;

	public bool IsConnectedClient { get; internal set; }

	public Action<uint> OnClientConnectedCallback = null;
	public Action<uint> OnClientDisconnectCallback = null;
	public Action<Entity> OnAvatarSpawn = null;
	public Action<Entity> OnEntitySpawn = null;







	/// <summary>
	/// Gets the currently in use certificate
	/// </summary>
	public X509Certificate2 ServerX509Certificate
	{
		get
		{
			return serverX509Certificate;
		}
		internal set
		{
			serverX509CertificateBytes = null;
			serverX509Certificate = value;
		}
	}
	private X509Certificate2 serverX509Certificate;
	/// <summary>
	/// Gets the cached binary representation of the server certificate that's used for handshaking
	/// </summary>
	public byte[] ServerX509CertificateBytes
	{
		get
		{
			if (serverX509CertificateBytes == null)
				serverX509CertificateBytes = ServerX509Certificate.Export(X509ContentType.Cert);
			return serverX509CertificateBytes;
		}
	}
	private byte[] serverX509CertificateBytes = null;





	/// <summary>
	/// Delegate used for incoming custom messages
	/// </summary>
	/// <param name="clientId">The clientId that sent the message</param>
	/// <param name="stream">The stream containing the message data</param>
	public delegate void CustomMessageDelegete(uint clientId, Stream stream);
	/// <summary>
	/// Event invoked when custom messages arrive
	/// </summary>
	public event CustomMessageDelegete OnIncomingCustomMessage;
	/// <summary>
	/// The current hostname we are connected to, used to validate certificate
	/// </summary>
	public string ConnectedHostname { get; private set; }

	internal byte[] clientAesKey;

	private uint entityIdCounter = 0;

	internal void InvokeOnIncomingCustomMessage(uint clientId, Stream stream)
	{
		if (OnIncomingCustomMessage != null) OnIncomingCustomMessage(clientId, stream);
	}

	/// <summary>
	/// Sends custom message to all clients
	/// </summary>
	/// <param name="stream">The message stream containing the data</param>
	/// <param name="channel">The channel to send the data on</param>
	/// <param name="security">The security settings to apply to the message</param>
	public void BroadcastCustomMessage( BitStream stream, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
	{
		if (!IsServer)
		{
			if (LogHelper.CurrentLogLevel <= LogLevel.Error) LogHelper.LogWarning("Only the server can broadcast custom messages");
			return;
		}
		for (int i = 0; i < _connectedClients.GetCount(); ++i)
		{
			InternalMessageHandler.Send( _connectedClients.GetAt(i).GetId(), AlpacaConstant.ALPACA_CUSTOM_MESSAGE, string.IsNullOrEmpty(channel) ? "ALPACA_DEFAULT_MESSAGE" : channel, stream, security);
		}
	}

	/// <summary>
	/// Sends a custom message to a specific client
	/// </summary>
	/// <param name="clientId">The client to send the message to</param>
	/// <param name="stream">The message stream containing the data</param>
	/// <param name="channel">The channel tos end the data on</param>
	/// <param name="security">The security settings to apply to the message</param>
	public void SendCustomMessage(uint clientId, BitStream stream, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
	{
		InternalMessageHandler.Send(clientId, AlpacaConstant.ALPACA_CUSTOM_MESSAGE, string.IsNullOrEmpty(channel) ? "ALPACA_DEFAULT_MESSAGE" : channel, stream, security);
	}

	// get the public key we need for encrypting a message to the target client, or server if we are a client
	public byte[] GetPublicEncryptionKey( uint clientId )
	{
		if( IsServer )
		{
			Client c = _connectedClients.Get( clientId );
			if( c != null ) { return c.AesKey; }
			
			PendingClient p = _pendingClients.Get( clientId );
			if( p != null ) { return p.AesKey; }

			return null;
		}
		else
		{
			return clientAesKey;
		}
		return null;
	}

	private object Init(bool server)
	{
		if (LogHelper.CurrentLogLevel <= LogLevel.Developer) LogHelper.LogInfo("Init()");
		LocalClientId = 0;
		NetworkTime = 0f;
		lastSendTickTime = 0f;
		lastEventTickTime = 0f;
		lastReceiveTickTime = 0f;
		eventOvershootCounter = 0f;

		_pendingClients = new PendingClientSet( config.MaxConnections );
		_connectedClients = new ClientSet( config.MaxConnections );

		messageBuffer = new byte[config.MessageBufferSize];
		
		ResponseMessageManager.Clear();
		MessageManager.channels.Clear();
		MessageManager.reverseChannels.Clear();
		SpawnManager.SpawnedObjects.Clear();
		SpawnManager.SpawnedObjectsList.Clear();
		//SpawnManager.releasedEntityIds.Clear();

		try
		{
			if (server && !string.IsNullOrEmpty(config.ServerBase64PfxCertificate))
			{
				config.ServerX509Certificate = new X509Certificate2(Convert.FromBase64String(config.ServerBase64PfxCertificate));
				if (!config.ServerX509Certificate.HasPrivateKey)
				{
					if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("The imported PFX file did not have a private key");
				}
			}
		}
		catch (CryptographicException ex)
		{
			if (LogHelper.CurrentLogLevel <= LogLevel.Error) LogHelper.LogError("Importing of certificate failed: " + ex.ToString());
		}

		if (config.Transport == DefaultTransport.UNET)
			config.NetworkTransport = new UnetTransport();

		object settings = config.NetworkTransport.GetSettings(); //Gets a new "settings" object for the transport currently used.

		List<Channel> internalChannels = new List<Channel>
		{
			new Channel()
			{
				Name = "ALPACA_INTERNAL",
				Type = config.NetworkTransport.InternalChannel
			},
			new Channel()
			{
				Name = "ALPACA_DEFAULT_MESSAGE",
				Type = ChannelType.Reliable
			},
			new Channel()
			{
				Name = "ALPACA_POSITION_UPDATE",
				Type = ChannelType.StateUpdate
			},
			new Channel()
			{
				Name = "ALPACA_ANIMATION_UPDATE",
				Type = ChannelType.ReliableSequenced
			},
			new Channel()
			{
				Name = "ALPACA_NAV_AGENT_STATE",
				Type = ChannelType.ReliableSequenced
			},
			new Channel()
			{
				Name = "ALPACA_NAV_AGENT_CORRECTION",
				Type = ChannelType.StateUpdate
			},
			new Channel()
			{
				Name = "ALPACA_TIME_SYNC",
				Type = ChannelType.Unreliable
			}
		};

		HashSet<string> channelNames = new HashSet<string>();
		// Register internal channels
		for (int i = 0; i < internalChannels.Count; i++)
		{
			if (channelNames.Contains(internalChannels[i].Name))
			{
				if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Duplicate channel name: " + config.Channels[i].Name);
				continue;
			}
			int channelId = config.NetworkTransport.AddChannel(internalChannels[i].Type, settings);
			MessageManager.channels.Add(internalChannels[i].Name, channelId);
			channelNames.Add(internalChannels[i].Name);
			MessageManager.reverseChannels.Add(channelId, internalChannels[i].Name);
		}

		//Register user channels
		config.Channels = config.Channels.OrderBy(x => x.Name).ToList();
		for (int i = 0; i < config.Channels.Count; i++)
		{
			if(channelNames.Contains(config.Channels[i].Name))
			{
				if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Duplicate channel name: " + config.Channels[i].Name);
				continue;
			}
			int channelId = config.NetworkTransport.AddChannel(config.Channels[i].Type, settings);
			MessageManager.channels.Add(config.Channels[i].Name, channelId);
			channelNames.Add(config.Channels[i].Name);
			MessageManager.reverseChannels.Add(channelId, config.Channels[i].Name);
		}

		return settings;
	}

	// Returns true on success
	public bool StartServer( out string errorString )
	{
		if (LogHelper.CurrentLogLevel <= LogLevel.Developer) LogHelper.LogInfo("StartServer()");
		if (IsServer || IsClient)
		{
			errorString = "Cannot start server while an instance is already running";
			return false;
		}

		if(  config.ConnectionApproval
			&& ConnectionApprovalCallback == null
			)
		{
			errorString = "No ConnectionApproval callback defined. Connection approval will timeout";
			return false;
		}

		object settings = Init(true);
		config.NetworkTransport.RegisterServerListenSocket(settings);

		IsServer = true;
		IsClient = false;
		IsListening = true;

		errorString = null;
		return true;
	}

	public void StopServer()
	{
		if (LogHelper.CurrentLogLevel <= LogLevel.Developer) LogHelper.LogInfo("StopServer()");

		// Don't know if I have to disconnect the clients. I'm assuming the NetworkTransport does all the cleaning on shutdown.
		// But this way the clients get a disconnect message from server (so long it does't get lost)
		for( int i = 0; i < _connectedClients.GetCount(); ++i )
		{
			Client c = _connectedClients.GetAt(i);
			if( c.GetId() != config.NetworkTransport.ServerClientId )
			{
				config.NetworkTransport.DisconnectClient( c.GetId() );
			}
		}
		for( int i = 0; i < _pendingClients.GetCount(); ++i )
		{
			PendingClient c = _pendingClients.GetAt(i);
			if( c.GetId() != config.NetworkTransport.ServerClientId )
			{
				config.NetworkTransport.DisconnectClient( c.GetId() );
			}
		}
		
		IsServer = false;
		Shutdown();
	}

	// Returns true on success
	public bool StartClient( out string errorString )
	{
		if (LogHelper.CurrentLogLevel <= LogLevel.Developer) LogHelper.LogInfo("StartClient()");
		
		if (IsServer || IsClient)
		{
			errorString = "Cannot start client while an instance is already running";
			return false;
		}

		object settings = Init(false);
		ConnectedHostname = config.ConnectAddress;

		byte errorByte;
		config.NetworkTransport.Connect(config.ConnectAddress, config.ConnectPort, settings, out errorByte );
		NetworkError error = (NetworkError)errorByte;

		if( error == NetworkError.Ok )
		{
			IsServer = false;
			IsClient = true;
			IsListening = true;

			errorString = null;
			return true;
		}
		else
		{
			errorString = "Encountered Unity.Networking.NetworkError: " +  Enum.GetNames(typeof(NetworkError))[(int)error];
			return false;
		}
	}

	public void StopClient()
	{
		if (LogHelper.CurrentLogLevel <= LogLevel.Developer) LogHelper.LogInfo("StopClient()");
		IsClient = false;
		config.NetworkTransport.DisconnectFromServer();
		IsConnectedClient = false;
		Shutdown();
	}

	private void OnEnable()
	{
		if( _singleton != null )
		{
			Debug.LogError( "Error: AlpacaNetwork already exists." );
		}
		else
		{
			_singleton = this;
			Application.runInBackground = true;
		}
	}
	
	private void OnDestroy()
	{
		if( _singleton != null && _singleton == this)
		{
			_singleton = null;
			Shutdown();  
		}
	}

	private void Shutdown()
	{
		if (LogHelper.CurrentLogLevel <= LogLevel.Developer) LogHelper.LogInfo("Shutdown()");
		NetworkProfiler.Stop();
		IsListening = false;
		IsServer = false;
		IsClient = false;
		SpawnManager.DestroyNonSceneObjects();

		if (config != null && config.NetworkTransport != null) //The Transport is set during Init time, thus it is possible for the Transport to be null
			config.NetworkTransport.Shutdown();
	}

	private float lastReceiveTickTime;
	private float lastSendTickTime;
	private float lastEventTickTime;
	private float eventOvershootCounter;
	private float lastTimeSyncTime;

	private void Update()
	{
		if(IsListening)
		{
			if((NetworkTime - lastSendTickTime >= (1f / config.SendTickrate)) || config.SendTickrate <= 0)
			{
				foreach( Entity obj in _networkedObjects )
				{
					obj.NetworkedVarUpdate();
				}

				for( int i = 0; i < _connectedClients.GetCount(); ++i )
				{
					uint clientId = _connectedClients.GetAt(i).GetId();
					byte error;
					config.NetworkTransport.SendQueue(clientId, out error);
					if (LogHelper.CurrentLogLevel <= LogLevel.Developer) LogHelper.LogInfo("Send Pending Queue: " + clientId);
				}

				lastSendTickTime = NetworkTime;
			}
			if((NetworkTime - lastReceiveTickTime >= (1f / config.ReceiveTickrate)) || config.ReceiveTickrate <= 0)
			{
				NetworkProfiler.StartTick(TickType.Receive);
				NetEventType eventType;
				int processedEvents = 0;
				do
				{
					processedEvents++;
					uint clientId;
					int channelId;
					int receivedSize;
					byte error;
					byte[] data = messageBuffer;
					eventType = config.NetworkTransport.PollReceive(out clientId, out channelId, ref data, data.Length, out receivedSize, out error);

					switch (eventType)
					{
						case NetEventType.Connect:
							NetworkProfiler.StartEvent(TickType.Receive, (uint)receivedSize, MessageManager.reverseChannels[channelId], "TRANSPORT_CONNECT");
							if (IsServer)
							{
								if (LogHelper.CurrentLogLevel <= LogLevel.Developer) LogHelper.LogInfo("Client Connected");
#if !DISABLE_CRYPTOGRAPHY
								if (config.EnableEncryption)
								{
									// This client is required to complete the crypto-hail exchange.
									using (PooledBitStream hailStream = PooledBitStream.Get())
									{
										using (PooledBitWriter hailWriter = PooledBitWriter.Get(hailStream))
										{
											if (config.SignKeyExchange)
											{
												// Write certificate
												hailWriter.WriteByteArray(config.ServerX509CertificateBytes);
											}

											// Write key exchange public part
											// TODO: cozeroff
											EllipticDiffieHellman diffieHellman = new EllipticDiffieHellman(EllipticDiffieHellman.DEFAULT_CURVE, EllipticDiffieHellman.DEFAULT_GENERATOR, EllipticDiffieHellman.DEFAULT_ORDER);
											byte[] diffieHellmanPublicPart = diffieHellman.GetPublicKey();
											hailWriter.WriteByteArray(diffieHellmanPublicPart);
											_pendingClients.Add(clientId, new PendingClient()
											{
												ClientId = clientId,
												ConnectionState = PendingClient.State.PendingHail,
												KeyExchange = diffieHellman
											});
											

											if (config.SignKeyExchange)
											{
												// Write public part signature (signed by certificate private)
												X509Certificate2 certificate = config.ServerX509Certificate;
												if (!certificate.HasPrivateKey) throw new CryptographicException("[Alpaca] No private key was found in server certificate. Unable to sign key exchange");
												RSACryptoServiceProvider rsa = certificate.PrivateKey as RSACryptoServiceProvider;

												if (rsa != null)
												{
													using (SHA256Managed sha = new SHA256Managed())
													{
														hailWriter.WriteByteArray(rsa.SignData(diffieHellmanPublicPart, sha));
													}
												}
												else
												{
													throw new CryptographicException("[Alpaca] Only RSA certificates are supported. No valid RSA key was found");
												}
											}
										}
										// Send the hail
										InternalMessageHandler.Send(clientId, AlpacaConstant.ALPACA_CERTIFICATE_HAIL, "ALPACA_INTERNAL", hailStream, SecuritySendFlags.None, true);
									}
								}
								else
								{
#endif
									// TODO: cozeroff
									_pendingClients.Add(clientId, new PendingClient()
									{
										ClientId = clientId,
										ConnectionState = PendingClient.State.PendingConnection
									});
									
#if !DISABLE_CRYPTOGRAPHY
								}
#endif
								StartCoroutine(ApprovalTimeout(clientId));
							}
							else
							{
								if (LogHelper.CurrentLogLevel <= LogLevel.Developer) LogHelper.LogInfo("Connected");
								if (!config.EnableEncryption) SendConnectionRequest();
								StartCoroutine(ApprovalTimeout(clientId));
							}
							NetworkProfiler.EndEvent();
							break;
						case NetEventType.Data:
							if (LogHelper.CurrentLogLevel <= LogLevel.Developer) LogHelper.LogInfo($"Incoming Data From {clientId} : {receivedSize} bytes");

							HandleIncomingData(clientId, data, channelId, receivedSize);
							break;
						case NetEventType.Disconnect:
							NetworkProfiler.StartEvent(TickType.Receive, 0, "NONE", "TRANSPORT_DISCONNECT");
							if (LogHelper.CurrentLogLevel <= LogLevel.Developer) LogHelper.LogInfo("Disconnect Event From " + clientId);

							if( IsServer )
							{
								OnClientDisconnectServer(clientId);
							}
							else
							{
								IsConnectedClient = false;
								StopClient();
							}

							if (OnClientDisconnectCallback != null)
								OnClientDisconnectCallback.Invoke(clientId);
							NetworkProfiler.EndEvent();
							break;
					}
					// Only do another iteration if: there are no more messages AND (there is no limit to max events or we have processed less than the maximum)
				} while (IsListening && (eventType != NetEventType.Nothing && (config.MaxReceiveEventsPerTickRate <= 0 || processedEvents < config.MaxReceiveEventsPerTickRate)));
				lastReceiveTickTime = NetworkTime;
				NetworkProfiler.EndTick();
			}

			if (IsServer && ((NetworkTime - lastEventTickTime >= (1f / config.EventTickrate))))
			{
				NetworkProfiler.StartTick(TickType.Event);
				eventOvershootCounter += ((NetworkTime - lastEventTickTime) - (1f / config.EventTickrate));
				LagCompensationManager.AddFrames();
				ResponseMessageManager.CheckTimeouts();
				lastEventTickTime = NetworkTime;
				NetworkProfiler.EndTick();
			}
			else if (IsServer && eventOvershootCounter >= ((1f / config.EventTickrate)))
			{
				NetworkProfiler.StartTick(TickType.Event);
				//We run this one to compensate for previous update overshoots.
				eventOvershootCounter -= (1f / config.EventTickrate);
				LagCompensationManager.AddFrames();
				NetworkProfiler.EndTick();
			}

			if (IsServer && config.EnableTimeResync && NetworkTime - lastTimeSyncTime >= 30)
			{
				NetworkProfiler.StartTick(TickType.Event);
				SyncTime();
				lastTimeSyncTime = NetworkTime;
				NetworkProfiler.EndTick();
			}

			NetworkTime += Time.unscaledDeltaTime;
		}
	}

	internal void SendConnectionRequest()
	{
		using (PooledBitStream stream = PooledBitStream.Get())
		{
			using (PooledBitWriter writer = PooledBitWriter.Get(stream))
			{
				writer.WriteUInt64Packed(config.GetHash());

				if( config.ConnectionApproval )
				{
					writer.WriteByteArray(config.ConnectionData);
				}
			}

			InternalMessageHandler.Send(ServerClientId, AlpacaConstant.ALPACA_CONNECTION_REQUEST, "ALPACA_INTERNAL", stream, SecuritySendFlags.Authenticated | SecuritySendFlags.Encrypted, true);
		}
	}

	private IEnumerator ApprovalTimeout(uint clientId)
	{
		float timeStarted = NetworkTime;
		// We yield every frame in case a pending client disconnects and someone else gets its connection id
		while (NetworkTime - timeStarted < config.ClientConnectionBufferTimeout && (_pendingClients.Get(clientId) != null) )
		{
			yield return null;
		}
		
		if( _pendingClients.Get(clientId) != null && _connectedClients.Get(clientId) == null )
		{
			// Timeout
			if (LogHelper.CurrentLogLevel <= LogLevel.Developer) LogHelper.LogInfo("Client " + clientId + " Handshake Timed Out");
			DisconnectClient(clientId);
		}
	}

	private void HandleIncomingData(uint clientId, byte[] data, int channelId, int totalSize)
	{
		if (LogHelper.CurrentLogLevel <= LogLevel.Developer) LogHelper.LogInfo("Unwrapping Data Header");

		using (BitStream inputStream = new BitStream(data))
		{
			inputStream.SetLength(totalSize);
			
			using (BitStream messageStream = MessageManager.UnwrapMessage(inputStream, clientId, out byte messageType, out SecuritySendFlags security))
			{
				if (messageStream == null)
				{
					if (LogHelper.CurrentLogLevel <= LogLevel.Error) LogHelper.LogError("Message unwrap could not be completed. Was the header corrupt? Crypto error?");
					return;
				}
				else if (messageType == AlpacaConstant.INVALID)
				{
					if (LogHelper.CurrentLogLevel <= LogLevel.Error) LogHelper.LogError("Message unwrap read an invalid messageType");
					return;
				}

				uint headerByteSize = (uint)Arithmetic.VarIntSize(messageType);
				NetworkProfiler.StartEvent(TickType.Receive, (uint)(totalSize - headerByteSize), channelId, messageType);

				if (LogHelper.CurrentLogLevel <= LogLevel.Developer) LogHelper.LogInfo("Data Header: messageType=" + messageType);

				// Pending client tried to send a network message that was not the connection request before he was accepted.
				PendingClient p = _pendingClients.Get(clientId);
				if( p != null ) // this will only ever be non-null if we are the server
				{
					// TODO: cozeroff
					if(  (p.ConnectionState == PendingClient.State.PendingHail       && messageType != AlpacaConstant.ALPACA_CERTIFICATE_HAIL_RESPONSE)
						|| (p.ConnectionState == PendingClient.State.PendingConnection && messageType != AlpacaConstant.ALPACA_CONNECTION_REQUEST       )
						)
					{
						if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Message received from clientId " + clientId + " before it has been accepted");
						return;
					}
				}

				#region INTERNAL MESSAGE

				switch (messageType)
				{
					case AlpacaConstant.ALPACA_CERTIFICATE_HAIL:
						if( IsClient ) { InternalMessageHandler.HandleHailRequest(clientId, messageStream, channelId); }
						break;
					case AlpacaConstant.ALPACA_CERTIFICATE_HAIL_RESPONSE:
						if( IsServer ) { InternalMessageHandler.HandleHailResponse(clientId, messageStream, channelId); }
						break;
					case AlpacaConstant.ALPACA_GREETINGS:
						if( IsClient ) { InternalMessageHandler.HandleGreetings(clientId, messageStream, channelId); }
						break;
					case AlpacaConstant.ALPACA_CONNECTION_REQUEST:
						if( IsServer ) { InternalMessageHandler.HandleConnectionRequest(clientId, messageStream, channelId); }
						break;
					case AlpacaConstant.ALPACA_CONNECTION_APPROVED:
						if( IsClient ) { InternalMessageHandler.HandleConnectionApprovedClient(clientId, messageStream, channelId); }
						break;
					case AlpacaConstant.ALPACA_CLIENT_DISCONNECT:
						if( IsClient ) { InternalMessageHandler.HandleClientDisconnectClient(clientId, messageStream, channelId); }
						break;
					case AlpacaConstant.ALPACA_ADD_OBJECT:
						if( IsClient ) { InternalMessageHandler.HandleAddObjectClient(clientId, messageStream, channelId); }
						break;
					case AlpacaConstant.ALPACA_ADD_OBJECTS:
						if( IsClient ) { InternalMessageHandler.HandleAddObjectsClient(clientId, messageStream, channelId); }
						break;
					case AlpacaConstant.ALPACA_DESTROY_OBJECT:
						if( IsClient ) { InternalMessageHandler.HandleDestroyObject(clientId, messageStream, channelId); }
						break;
					case AlpacaConstant.ALPACA_CHANGE_OWNER:
						if( IsClient ) { InternalMessageHandler.HandleChangeOwner(clientId, messageStream, channelId); }
						break;
					case AlpacaConstant.ALPACA_TIME_SYNC:
						if( IsClient ) { InternalMessageHandler.HandleTimeSync(clientId, messageStream, channelId); }
						break;
					case AlpacaConstant.ALPACA_NETWORKED_VAR_DELTA:
						InternalMessageHandler.HandleNetworkedVarDelta(clientId, messageStream, channelId);
						break;
					case AlpacaConstant.ALPACA_NETWORKED_VAR_UPDATE:
						InternalMessageHandler.HandleNetworkedVarUpdate(clientId, messageStream, channelId);
						break;
					case AlpacaConstant.ALPACA_SERVER_RPC:
						if( IsServer ) { InternalMessageHandler.HandleServerRPC(clientId, messageStream, channelId); }
						break;
					case AlpacaConstant.ALPACA_SERVER_RPC_REQUEST:
						if( IsServer ) { InternalMessageHandler.HandleServerRPCRequest(clientId, messageStream, channelId, security); }
						break;
					case AlpacaConstant.ALPACA_SERVER_RPC_RESPONSE:
						if( IsClient ) { InternalMessageHandler.HandleServerRPCResponse(clientId, messageStream, channelId); }
						break;
					case AlpacaConstant.ALPACA_CLIENT_RPC:
						if( IsClient ) { InternalMessageHandler.HandleClientRPC(clientId, messageStream, channelId); }
						break;
					case AlpacaConstant.ALPACA_CLIENT_RPC_REQUEST:
						if( IsClient ) { InternalMessageHandler.HandleClientRPCRequest(clientId, messageStream, channelId, security); }
						break;
					case AlpacaConstant.ALPACA_CLIENT_RPC_RESPONSE:
						if( IsServer ) { InternalMessageHandler.HandleClientRPCResponse(clientId, messageStream, channelId); }
						break;
					case AlpacaConstant.ALPACA_CUSTOM_MESSAGE:
						InternalMessageHandler.HandleCustomMessage(clientId, messageStream, channelId);
						break;
					default:
						LogHelper.LogError("Read unrecognized messageType " + messageType);
						break;
				}

				#endregion

				NetworkProfiler.EndEvent();
			}
		}
	}

#if NET45
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	internal void DisconnectClient(uint clientId)
	{
		if( !IsServer ) { return; }

		_connectedClients.Remove( clientId );
		_pendingClients.Remove( clientId );

		config.NetworkTransport.DisconnectClient(clientId);
	}

	internal void OnClientDisconnectClient( uint clientId )
	{
		Debug.Assert( IsClient );
		_connectedClients.Remove( clientId );
		return;
	}

	internal void OnClientDisconnectServer( uint clientId )
	{
		Debug.Assert( IsServer );
		bool didRemove = false;
		if( _pendingClients.Get( clientId ) != null)
		{
			didRemove = _pendingClients.Remove( clientId );
			Debug.Assert( didRemove );
		}
		else
		{
			Client c = _connectedClients.Get( clientId );
			Debug.Assert( c != null );
			if( c != null )
			{
				// destroy the avatar, it cannot be owned by anyone else
				Entity avatar = c.GetAvatar();
				if( avatar != null )
				{
					// TODO: should clean up references here before destroying it?
					Destroy( avatar.gameObject );
					avatar = null;
				}

				// return ownership of all other entities to the server
				EntitySet ownedObjects = c.GetOwnedEntity();
				for( int i = 0; i < ownedObjects.GetCount(); ++i )
				{
					Entity e = ownedObjects.GetAt(i);
					e.SetOwnerClientId( ServerClientId );

					// I think separate streams are needed for each message
					using( PooledBitStream stream = PooledBitStream.Get() )
					using( PooledBitWriter writer = PooledBitWriter.Get(stream) )
					{
						writer.WriteUInt32Packed( e.GetId() );
						writer.WriteUInt32Packed( e.GetOwnerClientId() );

						InternalMessageHandler.Send( AlpacaConstant.ALPACA_CHANGE_OWNER, "ALPACA_INTERNAL", stream, SecuritySendFlags.None );
					}
				}

				didRemove = _connectedClients.Remove( clientId );
				Debug.Assert( didRemove );
			}
		}

		if( didRemove )
		{
			using( PooledBitStream stream = PooledBitStream.Get() )
			using( PooledBitWriter writer = PooledBitWriter.Get(stream) )
			{
				writer.WriteUInt32Packed( clientId );
				InternalMessageHandler.Send( AlpacaConstant.ALPACA_CLIENT_DISCONNECT, "ALPACA_INTERNAL", clientId, stream, SecuritySendFlags.None );
			}
		}
	}

	private void SyncTime()
	{
		if (LogHelper.CurrentLogLevel <= LogLevel.Developer) LogHelper.LogInfo("Syncing Time To Clients");
		using (PooledBitStream stream = PooledBitStream.Get())
		{
			using (PooledBitWriter writer = PooledBitWriter.Get(stream))
			{
				writer.WriteSinglePacked(NetworkTime);
				int timestamp = config.NetworkTransport.GetNetworkTimestamp();
				writer.WriteInt32Packed(timestamp);
				InternalMessageHandler.Send(AlpacaConstant.ALPACA_TIME_SYNC, "ALPACA_TIME_SYNC", stream, SecuritySendFlags.None);
			}
		}
	}

	internal void HandleApproval(uint clientId, bool approved)
	{
		if( !approved )
		{
			_pendingClients.Remove( clientId );
			config.NetworkTransport.DisconnectClient(clientId);
		}

		// Inform new client it got approved

		PendingClient p = _pendingClients.Get( clientId );
		byte[] aesKey = p != null ? p.AesKey : null;
		_pendingClients.Remove( clientId );

		Client client = new Client()
		{
			ClientId = clientId,
#if !DISABLE_CRYPTOGRAPHY
			AesKey = aesKey
#endif
		};
		_connectedClients.Add(clientId, client);

		int amountOfObjectsToSend = SpawnManager.SpawnedObjects.Values.Count;

		using( PooledBitStream stream = PooledBitStream.Get() )
		using( PooledBitWriter writer = PooledBitWriter.Get(stream) )
		{
			writer.WriteUInt32Packed(clientId);
			writer.WriteSinglePacked(NetworkTime);
			writer.WriteInt32Packed(config.NetworkTransport.GetNetworkTimestamp());
			writer.WriteInt32Packed(amountOfObjectsToSend);

			foreach (KeyValuePair<uint, Entity> pair in SpawnManager.SpawnedObjects)
			{
				// TODO: cozeroff
				writer.WriteBool(pair.Value.IsAvatar());
				writer.WriteUInt32Packed(pair.Value.GetId());
				writer.WriteUInt32Packed(pair.Value.GetOwnerClientId());
				writer.WriteUInt64Packed(pair.Value.NetworkedPrefabHash);
				writer.WriteBool(pair.Value.gameObject.activeInHierarchy);

				writer.WriteSinglePacked(pair.Value.transform.position.x);
				writer.WriteSinglePacked(pair.Value.transform.position.y);
				writer.WriteSinglePacked(pair.Value.transform.position.z);

				writer.WriteSinglePacked(pair.Value.transform.rotation.eulerAngles.x);
				writer.WriteSinglePacked(pair.Value.transform.rotation.eulerAngles.y);
				writer.WriteSinglePacked(pair.Value.transform.rotation.eulerAngles.z);

				pair.Value.WriteNetworkedVarData( writer, clientId);
			}

			InternalMessageHandler.Send(clientId, AlpacaConstant.ALPACA_CONNECTION_APPROVED, "ALPACA_INTERNAL", stream, SecuritySendFlags.Encrypted | SecuritySendFlags.Authenticated, true);

			if( OnClientConnectedCallback != null ) { OnClientConnectedCallback.Invoke(clientId); }
		}

		// Inform old clients of the new player

		for( int i = 0; i < _connectedClients.GetCount(); ++i )
		{
			Client c = _connectedClients.GetAt(i);
			if( c.GetId() == clientId )
			{
				continue; // the new client
			}

			using (PooledBitStream stream = PooledBitStream.Get())
			{
				using (PooledBitWriter writer = PooledBitWriter.Get(stream))
				{
					writer.WriteUInt32Packed(clientId);
					InternalMessageHandler.Send( c.GetId(), AlpacaConstant.ALPACA_ADD_OBJECT, "ALPACA_INTERNAL", stream, SecuritySendFlags.None );
				}
			}
		}
	}

	public System.UInt64 HashString( string name )
	{
		HashSize mode = config.StringHashSize;
		switch( mode )
		{
			case HashSize.TwoBytes:
				return name.GetStableHash16();
			case HashSize.FourBytes:
				return name.GetStableHash32();
			case HashSize.EightBytes:
				return name.GetStableHash64();
			default:
				Debug.Assert(false);
				return 0;
		}
	}

	// Server only
	public Entity SpawnEntityServer( uint ownerClientId, int prefabIndex, bool isAvatar, Vector3 position, Quaternion rotation )
	{
		Debug.Assert( IsServer );

		Client client = null;
		if( ownerClientId != ServerClientId )
		{
			client = _connectedClients.Get( ownerClientId );
			if( client == null )
			{
				LogHelper.LogError( "Cannot spawn entity with ownerClientId " + ownerClientId + ", client not yet connected!" );
				return null;
			}

			if( isAvatar && client.GetAvatar() != null )
			{
				LogHelper.LogError("Cannot spawn avatar entity. Client " + ownerClientId + " already has an avatar" );
				return null;
			}
		}

		// Generate unique network id
		uint netId = GetEntityId();

		// spawn
		Entity.Spawn data = new Entity.Spawn( netId, ownerClientId, prefabIndex, isAvatar, position, rotation );
		Entity entity = Entity.SpawnEntity( this, data );

		// send spawn notification to all clients
		for( int i = 0; i < _connectedClients.GetCount(); ++i )
		{
			Client targetClient = _connectedClients.GetAt(i);
			using( PooledBitStream stream = PooledBitStream.Get() )
			using( PooledBitWriter writer = PooledBitWriter.Get(stream) )
			{
				data.WriteTo( writer );
				// TODO cozeroff: write networked var data here
				InternalMessageHandler.Send( targetClient.GetId(), AlpacaConstant.ALPACA_ADD_OBJECT, "ALPACA_INTERNAL", stream, SecuritySendFlags.None );
			}
		}

		return entity;
	}

	public int FindPrefabIndex( Entity prefab )
	{
		Debug.Assert( prefab.gameObject.scene.name == null, "Must search for a prefab" );
		for( int i = 0; i < config.NetworkedPrefabs.Count(); ++i )
		{
			if( config.NetworkedPrefabs[i] == prefab )
			{
				return i;
			}
		}

		return AlpacaConstant.PREFAB_INDEX_INVALID;
	}


	// PRIVATE


	uint GetEntityId()
	{
		return ++entityIdCounter;
	}
	*/
}

} // namespace Alpaca