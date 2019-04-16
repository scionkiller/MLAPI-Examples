using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Networking;

using Alpaca.Serialization;
using Alpaca.Cryptography;
using InternalMessage = Alpaca.AlpacaConstant.InternalMessage;
using MessageSecurity = Alpaca.AlpacaConstant.MessageSecurity;


namespace Alpaca
{

public class ServerNodeSettings : MonoBehaviour
{
	public int clientHandshakeTimeout = 10;
	public int maxConnections = 64;
}

// ignore Obsolete warning for UNET
#pragma warning disable 618

public class ServerNode : CommonNode
{
	#region INTERNAL CLASSES

	private class ClientConnection
	{
		public enum State
		{
			  PendingCryptoHail          // doing the crypto handshake
			, PendingConnectionRequest   // waiting for internal connection message
			, Connected
		}

		NodeIndex _id;
		State _state;
		EllipticDiffieHellman _keyExchange;
		byte[] _sharedSecretKey; // AES-256
		Entity _playerAvatar;
		EntitySet _ownedEntity;


		public ClientConnection( NodeIndex id, State state, EllipticDiffieHellman keyExchange )
		{
			_id = id;
			_state = state;
			_keyExchange = keyExchange;
			_sharedSecretKey = null;
			_playerAvatar = null;
			_ownedEntity = new EntitySet( AlpacaConstant.CLIENT_OWNER_LIMIT );
		}

		/* TODO: cozeroff review this, this specific ctor probably never called
		public ClientConnection( NodeIndex id, Entity playerAvatar, byte[] sharedSecretKey )
		{
			_id = id;
			_playerAvatar = playerAvatar;

			_ownedEntity = new EntitySet( AlpacaConstant.CLIENT_OWNER_LIMIT );
			_ownedEntity.Add( _playerAvatar.GetIndex(), _playerAvatar );

			_sharedSecretKey = sharedSecretKey;
		}*/

		public State GetState() { return _state; }
		public bool IsConnected() { return _state == State.Connected; }
		public void SetConnected() { _state = State.Connected; }

		public NodeIndex GetIndex() { return _id; }
		public Entity GetAvatar() { return _playerAvatar; }

		public byte[] GetSharedSecretKey() { return _sharedSecretKey; }

		public int GetOwnedEntityCount() { return _ownedEntity.GetCount(); }
		
		public Entity GetOwnedEntityAt( int index )
		{ 
			return _ownedEntity.GetAt( index );
		}

		public void AddOwnedEntity( Entity entity )
		{
			_ownedEntity.Add( entity.GetIndex(), entity );
		}

		public void RemoveOwnedEntity( Entity entity )
		{
			_ownedEntity.Remove( entity.GetIndex() );
		}
	}

	private class ConnectionSet : ArraySet<NodeIndex, ClientConnection>
	{
		public ConnectionSet( int capacity ) : base(capacity) {}
	}

	#endregion // INTERNAL CLASSES


	ServerNodeSettings _serverSettings;
	bool _isRunning;

	// simple counters used to make sure that EntityId and ConductIds are kept unique
	uint _entityCounter;
	uint _conductCounter;




	// TODO: cozeroff
	private float eventOvershootCounter;
	private float lastTimeSyncTime;




	ConnectionSet _connection;
	EntitySet _entity;

	// callbacks
	System.Action<NodeIndex> _onClientConnect = null;
	System.Action<NodeIndex> _onClientDisconnect = null;
	
	public void SetOnClientConnect   ( System.Action<NodeIndex> callback            ) { _onClientConnect    = callback; }
	public void SetOnClientDisconnect( System.Action<NodeIndex> callback            ) { _onClientDisconnect = callback; }

	public bool IsRunning() { return _isRunning; }


	public ServerNode( CommonNodeSettings commonSettings, ServerNodeSettings serverSettings ) : base( commonSettings )
	{
		_serverSettings = serverSettings;
		_isRunning = false;

		_networkTime = 0f;
		_localIndex = NodeIndex.SERVER_NODE_INDEX;

		_entityCounter = 0;
		_conductCounter = 0;

		_connection = new ConnectionSet( _serverSettings.maxConnections );

		// TODO: cozeroff

		/* 
		eventOvershootCounter = 0f;
		
		ResponseMessageManager.Clear();
		SpawnManager.SpawnedObjects.Clear();
		SpawnManager.SpawnedObjectsList.Clear();

		try
		{
			if (server && !string.IsNullOrEmpty(config.ServerBase64PfxCertificate))
			{
				config.ServerX509Certificate = new X509Certificate2(Convert.FromBase64String(config.ServerBase64PfxCertificate));
				if (!config.ServerX509Certificate.HasPrivateKey)
				{
					Log.Warn("The imported PFX file did not have a private key");
				}
			}
		}
		catch (CryptographicException ex)
		{
			Log.Error("Importing of certificate failed: " + ex.ToString());
		}
		*/
		
	}

	public override bool Start( out string error )
	{
		ConnectionConfig config = InitializeNetwork( out error );
		if( config == null )
		{
			return false;
		}

		HostTopology topology = new HostTopology( config, _serverSettings.maxConnections );
		int portId = NetworkTransport.AddHost( topology, GetConnectionPort() );

		_isRunning = true;
		return true;
	}

	public void UpdateServer()
	{
		Debug.Assert( _isRunning );

		// Phase 1: Have conducts update their SyncVars
		// TODO: cozeroff
		// foreach Type t in _conductTypes
		// 	foreach Conduct c in _ownedConducts[t]
		// 		bool sendNeeded = c.LocalAct();
		// 		if( sendNeeded )
		// 		{
		// 			foreach Client client in _clients   // only includes nodes other than us
		// 				c.Write( client )
		// 		}

		// 	foreach Conduct c in _unownedConducts[t]
		// 		c.RemotePredict();

		// Phase 2: Check for networks packets received
		//_profiler.StartTick( TickType.Receive );
		ReceiveEvent eventType;
		int processedEvents = 0;
		do
		{
			++processedEvents;
			eventType = HandleNetworkEvent();
		} while 
		(  (eventType != ReceiveEvent.NoMoreEvents) 
		&& (eventType != ReceiveEvent.Error)
		&& (processedEvents < _maxEventCount)
		);
		//_profiler.EndTick();

		// Phase 3: apply all conducts, allows them to respond to data changes
		// TODO: cozeroff
		// foreach Type t in _conductTypes
		// 	foreach Conduct c in _ownedConducts[t]
		// 		c.LocalApply()

		// 	foreach Conduct c in _unownedConducts[t]
		// 		c.RemoteApply();

		_networkTime += Time.unscaledDeltaTime;
	}

	/*
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
													using (SHA256Managed sha = new SHA256Managed())4
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
									// TODO: cozeroff
									_pendingClients.Add(clientId, new PendingClient()
									{
										ClientId = clientId,
										ConnectionState = PendingClient.State.PendingConnection
									});
									
								}
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
	*/

	public EntityPrefabIndex FindEntityPrefabIndex( Entity prefab )
	{
		for( uint i = 0; i < _commonSettings.entity.Length; ++i )
		{
			if( _commonSettings.entity[i] == prefab )
			{
				return new EntityPrefabIndex(i);
			}
		}

		return new EntityPrefabIndex();
	}


	// PRIVATE

	ReceiveEvent HandleNetworkEvent()
	{
		BitReader reader = GetPooledReader();
		DataStream stream = reader.GetStream();

		int connectionId;
		ChannelIndex channel;
		ReceiveEvent e = PollReceive( stream, out connectionId, out channel );

		if( e == ReceiveEvent.Error ) { return e; }

		// on the server, the connectionId is one to one with the client index
		NodeIndex client = new NodeIndex( (uint)connectionId );

		switch( e )
		{
			case ReceiveEvent.Connect:
				//_profiler.RecordEvent(TickType.Receive, (uint)receivedSize, MessageManager.reverseChannels[channelId], "TRANSPORT_CONNECT");
				Log.Info( "Client sent initial connection packet" );
				if( _commonSettings.enableEncryption )
				{
					// This client is required to complete the crypto-hail exchange.
					EllipticDiffieHellman keyExchange = SendCryptoHail();

					ClientConnection c = new ClientConnection( client, ClientConnection.State.PendingCryptoHail, keyExchange );
					_connection.Add( c.GetIndex(), c );
				}
				else
				{
					ClientConnection c = new ClientConnection( client, ClientConnection.State.PendingConnectionRequest, null );		
				}
				// TODO: cozeroff NO, NO, NO, check for timeouts of clients manually
				//StartCoroutine(ApprovalTimeout(clientId));
				break;

			case ReceiveEvent.Message:
				HandleMessage( reader, client, channel );
				break;
				
			case ReceiveEvent.Disconnect:
				//_profiler.RecordEvent(TickType.Receive, 0, "NONE", "TRANSPORT_DISCONNECT");
				Log.Info( "Disconnect Event From " + client.GetClientIndex() );

				// TODO: cozeroff
				//OnClientDisconnectServer( client );

				if( _onClientDisconnect != null )
				{
					_onClientDisconnect.Invoke( client );
				}
				break;
		}

		return e;
	}

	EllipticDiffieHellman SendCryptoHail()
	{
		// TODO: cozeroff
		// using( PooledBitStream hailStream = PooledBitStream.Get() )
		// using( PooledBitWriter hailWriter = PooledBitWriter.Get(hailStream) )
		// {
		// 	if (config.SignKeyExchange)
		// 	{
		// 		// Write certificate
		// 		hailWriter.WriteByteArray(config.ServerX509CertificateBytes);
		// 	}

		// 	// Write key exchange public part
		// 	EllipticDiffieHellman diffieHellman = new EllipticDiffieHellman(EllipticDiffieHellman.DEFAULT_CURVE, EllipticDiffieHellman.DEFAULT_GENERATOR, EllipticDiffieHellman.DEFAULT_ORDER);
		// 	byte[] diffieHellmanPublicPart = diffieHellman.GetPublicKey();
		// 	hailWriter.WriteByteArray(diffieHellmanPublicPart);

		// 	if (config.SignKeyExchange)
		// 	{
		// 		// Write public part signature (signed by certificate private)
		// 		X509Certificate2 certificate = config.ServerX509Certificate;
		// 		if (!certificate.HasPrivateKey) throw new CryptographicException("[Alpaca] No private key was found in server certificate. Unable to sign key exchange");
		// 		RSACryptoServiceProvider rsa = certificate.PrivateKey as RSACryptoServiceProvider;

		// 		if (rsa != null)
		// 		{
		// 			using (SHA256Managed sha = new SHA256Managed())4
		// 			{
		// 				hailWriter.WriteByteArray(rsa.SignData(diffieHellmanPublicPart, sha));
		// 			}
		// 		}
		// 		else
		// 		{
		// 			throw new CryptographicException("[Alpaca] Only RSA certificates are supported. No valid RSA key was found");
		// 		}
		// 	}
		// }

		// InternalMessageHandler.Send(clientId, AlpacaConstant.ALPACA_CERTIFICATE_HAIL, "INTERNAL_CHANNEL_RELIABLE", hailStream, SecuritySendFlags.None, true);

		return null;
	}

	void HandleMessage( BitReader reader, NodeIndex client, ChannelIndex channel )
	{
		InternalMessage messageType = UnwrapMessage( reader, client );
		if( messageType == InternalMessage.INVALID ) { return; }

		//_profiler.StartEvent(TickType.Receive, size, channelId, messageType);

		Log.Info( $"Handling message {AlpacaConstant.GetName(messageType)} from client {client.GetClientIndex()}" );

		ClientConnection connection = _connection.GetAt( client.GetClientIndex() );
		ClientConnection.State state = connection.GetState();

		if( (state == ClientConnection.State.PendingCryptoHail) && (messageType != InternalMessage.CryptoHailResponse ) )
		{
			Log.Error( $"Client {client.GetClientIndex()} is pending crypto hail, but client sent message {AlpacaConstant.GetName(messageType)} instead." );
			return;
		}
		
		if( (state == ClientConnection.State.PendingConnectionRequest) && (messageType != InternalMessage.ConnectionRequest) )
		{
			Log.Error( $"Client {client.GetClientIndex()} is pending connection request, but client sent message {AlpacaConstant.GetName(messageType)} instead." );
			return;
		}

		switch( messageType )
		{
			case InternalMessage.CryptoHailResponse:
				// TODO: cozeroff
				//HandleHailResponse(clientId, messageStream, channelId);
				break;
			case InternalMessage.ConnectionRequest:
				HandleConnectionRequest( reader, client );
				break;
			case InternalMessage.SyncVarDelta:
				// TODO: cozeroff
				//HandleNetworkedVarDelta(clientId, messageStream, channelId);
				break;
			case InternalMessage.SyncVarUpdate:
				// TODO: cozeroff
				//HandleNetworkedVarUpdate(clientId, messageStream, channelId);
				break;
			case InternalMessage.Custom:
				// TODO: cozeroff
				//HandleCustomMessage(clientId, messageStream, channelId);
				break;
			default:
				Log.Error( $"Read unrecognized messageType{AlpacaConstant.GetName(messageType)}" );
				break;
		}

		//_profiler.EndEvent();
	}

	#region Message Handlers for specific InteralMessage types

	void HandleConnectionRequest( BitReader reader, NodeIndex client )
	{
		UInt64 configurationHash = reader.Packed<UInt64>();
		// TODO: find out why this config comparison fails when built on different machines, and restore this safety check
		// if(  !netManager.config.CompareConfig(configHash) )
		// {
		//	 Log.Warn("NetworkConfiguration mismatch. The configuration between the server and client does not match");
		//	 netManager.DisconnectClient(clientId);
		//	 return;
		// }

		// update ClientConnection state
		int clientIndex = client.GetClientIndex();
		ClientConnection connection = _connection.GetAt( clientIndex );
		connection.SetConnected();

		// send the new client the data it needs, plus the list of currently spawned items
		using( BitWriter writer = GetPooledWriter() )
		{
			writer.WriteUInt32Packed(clientId);
			writer.WriteSinglePacked(NetworkTime);
			writer.WriteInt32Packed(config.NetworkTransport.GetNetworkTimestamp());

			int amountOfObjectsToSend = _entity.GetCount();
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

			Send(clientId, AlpacaConstant.ALPACA_CONNECTION_APPROVED, "INTERNAL_CHANNEL_RELIABLE", stream, SecuritySendFlags.Encrypted | SecuritySendFlags.Authenticated, true);
		}

		// Inform old clients of the new player
		for( int i = 0; i < _connection.GetCount(); ++i )
		{
			if( i == clientIndex ) { continue; } // skip the new client

			ClientConnection other = _connection.GetAt(i);
			using( BitWriter writer = GetPooledWriter() )
			{
				writer.WriteUInt32Packed(clientId);
				Send( client, AlpacaConstant.ALPACA_ADD_OBJECT, "INTERNAL_CHANNEL_RELIABLE", stream, SecuritySendFlags.None );
			}
		}

		// callback
		if( _onClientConnect != null ) { _onClientConnect.Invoke( client ); }
	}

	#endregion // Message Handlers
}

} // namespace Alpaca