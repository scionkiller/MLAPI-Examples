/*


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.IO;
using System.Security.Cryptography.X509Certificates;

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

using Alpaca.Components;
using Alpaca.Cryptography;
using Alpaca.Data;
using Alpaca.Internal;
using Alpaca.Profiling;
using Alpaca.Serialization;
using Alpaca.Transports;
using BitStream = Alpaca.Serialization.BitStream;





public class AlpacaNetwork
{
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
	/// The current hostname we are connected to, used to validate certificate
	/// </summary>
	public string ConnectedHostname { get; private set; }

	internal byte[] clientAesKey;




	









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
			Log.Warn("Only the server can broadcast custom messages");
			return;
		}
		for (int i = 0; i < _connectedClients.GetCount(); ++i)
		{
			InternalMessageHandler.Send( _connectedClients.GetAt(i).GetId(), AlpacaConstant.ALPACA_CUSTOM_MESSAGE, string.IsNullOrEmpty(channel) ? "INTERNAL_CHANNEL_CLIENT_RELIABLE" : channel, stream, security);
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
		InternalMessageHandler.Send(clientId, AlpacaConstant.ALPACA_CUSTOM_MESSAGE, string.IsNullOrEmpty(channel) ? "INTERNAL_CHANNEL_CLIENT_RELIABLE" : channel, stream, security);
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

	// Returns true on success
	public bool StartServer( out string errorString )
	{
		Log.Info("StartServer()");
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
		Log.Info("StopServer()");

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
		Log.Info("StartClient()");
		
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
		Log.Info("StopClient()");
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
		Log.Info("Shutdown()");
		NetworkProfiler.Stop();
		IsListening = false;
		IsServer = false;
		IsClient = false;
		SpawnManager.DestroyNonSceneObjects();

		if (config != null && config.NetworkTransport != null) //The Transport is set during Init time, thus it is possible for the Transport to be null
			config.NetworkTransport.Shutdown();
	}

	

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
					Log.Info("Send Pending Queue: " + clientId);
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
								Log.Info("Client Connected");
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
										InternalMessageHandler.Send(clientId, AlpacaConstant.ALPACA_CERTIFICATE_HAIL, "INTERNAL_CHANNEL_RELIABLE", hailStream, SecuritySendFlags.None, true);
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
								Log.Info("Connected");
								if (!config.EnableEncryption) SendConnectionRequest();
								StartCoroutine(ApprovalTimeout(clientId));
							}
							NetworkProfiler.EndEvent();
							break;
						case NetEventType.Data:
							Log.Info($"Incoming Data From {clientId} : {receivedSize} bytes");

							HandleIncomingData(clientId, data, channelId, receivedSize);
							break;
						case NetEventType.Disconnect:
							NetworkProfiler.StartEvent(TickType.Receive, 0, "NONE", "TRANSPORT_DISCONNECT");
							Log.Info("Disconnect Event From " + clientId);

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

			InternalMessageHandler.Send(ServerClientId, AlpacaConstant.ALPACA_CONNECTION_REQUEST, "INTERNAL_CHANNEL_RELIABLE", stream, SecuritySendFlags.Authenticated | SecuritySendFlags.Encrypted, true);
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
			Log.Info("Client " + clientId + " Handshake Timed Out");
			DisconnectClient(clientId);
		}
	}

	private void HandleIncomingData(uint clientId, byte[] data, int channelId, int totalSize)
	{
		Log.Info("Unwrapping Data Header");

		using (BitStream inputStream = new BitStream(data))
		{
			inputStream.SetLength(totalSize);
			
			using (BitStream messageStream = MessageManager.UnwrapMessage(inputStream, clientId, out byte messageType, out SecuritySendFlags security))
			{
				if (messageStream == null)
				{
					Log.Error("Message unwrap could not be completed. Was the header corrupt? Crypto error?");
					return;
				}
				else if (messageType == AlpacaConstant.INVALID)
				{
					Log.Error("Message unwrap read an invalid messageType");
					return;
				}

				uint headerByteSize = (uint)Arithmetic.VarIntSize(messageType);
				NetworkProfiler.StartEvent(TickType.Receive, (uint)(totalSize - headerByteSize), channelId, messageType);

				Log.Info("Data Header: messageType=" + messageType);

				// Pending client tried to send a network message that was not the connection request before he was accepted.
				PendingClient p = _pendingClients.Get(clientId);
				if( p != null ) // this will only ever be non-null if we are the server
				{
					// TODO: cozeroff
					if(  (p.ConnectionState == PendingClient.State.PendingHail       && messageType != AlpacaConstant.ALPACA_CERTIFICATE_HAIL_RESPONSE)
						|| (p.ConnectionState == PendingClient.State.PendingConnection && messageType != AlpacaConstant.ALPACA_CONNECTION_REQUEST       )
						)
					{
						Log.Warn("Message received from clientId " + clientId + " before it has been accepted");
						return;
					}
				}

				#region INTERNAL MESSAGE

				switch( messageType )
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
					case AlpacaConstant.ALPACA_CUSTOM_MESSAGE:
						InternalMessageHandler.HandleCustomMessage(clientId, messageStream, channelId);
						break;
					default:
						Log.Error("Read unrecognized messageType " + messageType);
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

						InternalMessageHandler.Send( AlpacaConstant.ALPACA_CHANGE_OWNER, "INTERNAL_CHANNEL_RELIABLE", stream, SecuritySendFlags.None );
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
				InternalMessageHandler.Send( AlpacaConstant.ALPACA_CLIENT_DISCONNECT, "INTERNAL_CHANNEL_RELIABLE", clientId, stream, SecuritySendFlags.None );
			}
		}
	}

	private void SyncTime()
	{
		Log.Info("Syncing Time To Clients");
		using (PooledBitStream stream = PooledBitStream.Get())
		{
			using (PooledBitWriter writer = PooledBitWriter.Get(stream))
			{
				writer.WriteSinglePacked(NetworkTime);
				int timestamp = config.NetworkTransport.GetNetworkTimestamp();
				writer.WriteInt32Packed(timestamp);
				InternalMessageHandler.Send(AlpacaConstant.ALPACA_TIME_SYNC, "INTERNAL_CHANNEL_UNRELIABLE", stream, SecuritySendFlags.None);
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

			InternalMessageHandler.Send(clientId, AlpacaConstant.ALPACA_CONNECTION_APPROVED, "INTERNAL_CHANNEL_RELIABLE", stream, SecuritySendFlags.Encrypted | SecuritySendFlags.Authenticated, true);

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
					InternalMessageHandler.Send( c.GetId(), AlpacaConstant.ALPACA_ADD_OBJECT, "INTERNAL_CHANNEL_RELIABLE", stream, SecuritySendFlags.None );
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
				Log.Error( "Cannot spawn entity with ownerClientId " + ownerClientId + ", client not yet connected!" );
				return null;
			}

			if( isAvatar && client.GetAvatar() != null )
			{
				Log.Error("Cannot spawn avatar entity. Client " + ownerClientId + " already has an avatar" );
				return null;
			}
		}

		// Generate unique network id
		uint netId = GetEntityId();

		// spawn
		Entity.Spawn data = new Entity.Spawn( netId, ownerClientId, prefabIndex, isAvatar, position, rotation );
		Entity entity = Entity.SpawnEntity( this, data );

		// TODO: cozeroff implement this
		//network.AddEntity(entity.GetId(), entity);

		// send spawn notification to all clients
		for( int i = 0; i < _connectedClients.GetCount(); ++i )
		{
			Client targetClient = _connectedClients.GetAt(i);
			using( PooledBitStream stream = PooledBitStream.Get() )
			using( PooledBitWriter writer = PooledBitWriter.Get(stream) )
			{
				data.WriteTo( writer );
				// TODO cozeroff: write networked var data here
				InternalMessageHandler.Send( targetClient.GetId(), AlpacaConstant.ALPACA_ADD_OBJECT, "INTERNAL_CHANNEL_RELIABLE", stream, SecuritySendFlags.None );
			}
		}

		return entity;
	}


	// PRIVATE


	uint GetEntityId()
	{
		return ++entityIdCounter;
	}
}

} // namespace Alpaca


*/