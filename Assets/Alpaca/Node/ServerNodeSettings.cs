using System;

using UnityEngine;
using UnityEngine.Networking;

using Alpaca.Cryptography;
using Alpaca.Serialization;
using InternalMessage = Alpaca.AlpacaConstant.InternalMessage;
using InternalChannel = Alpaca.AlpacaConstant.InternalChannel;
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
		public enum Status
		{
			  PendingConnectionChallengeResponse  // doing the crypto handshake
			, PendingConnectionRequest            // waiting for internal connection message   
			, Connected
		}

		NodeIndex _id;
		Status _state;
		EllipticDiffieHellman _keyExchange;
		byte[] _sharedSecretKey; // AES-256
		Entity _playerAvatar;
		EntitySet _ownedEntity;


		public ClientConnection( NodeIndex id, Status state, EllipticDiffieHellman keyExchange )
		{
			_id = id;
			_state = state;
			_keyExchange = keyExchange;
			_sharedSecretKey = null;
			_playerAvatar = null;
			_ownedEntity = new EntitySet( AlpacaConstant.CLIENT_OWNER_LIMIT );
		}

		public NodeIndex GetId() { return _id; } 
		public Status GetState() { return _state; }
		public bool IsConnected() { return _state == Status.Connected; }
		public void SetConnected() { _state = Status.Connected; }

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

	ConnectionSet _connection;
	EntitySet _entity;

	// simple counters used to make sure that EntityId and ConductIds are kept unique
	uint _entityCounter;
	uint _conductCounter;

	// TODO: cozeroff
	private float eventOvershootCounter;
	private float lastTimeSyncTime;
	// END TODO

	// callbacks
	Action<NodeIndex> _onClientConnect = null;
	Action<NodeIndex> _onClientDisconnect = null;
	Action<NodeIndex, BitReader> _onMessageCustomServer = null;
	

	public void SetOnClientConnect      ( Action<NodeIndex> callback            ) { _onClientConnect       = callback; }
	public void SetOnClientDisconnect   ( Action<NodeIndex> callback            ) { _onClientDisconnect    = callback; }
	public void SetOnMessageCustomServer( Action<NodeIndex, BitReader> callback ) { _onMessageCustomServer = callback; }

	public bool IsRunning() { return _isRunning; }

	public ServerNode( CommonNodeSettings commonSettings, ServerNodeSettings serverSettings ) : base( commonSettings )
	{
		_serverSettings = serverSettings;
		_isRunning = false;

		_networkTime = 0f;
		_localIndex = NodeIndex.SERVER_NODE_INDEX;

		_entity = new EntitySet( 64 );
		_entityCounter = 0;
		_conductCounter = 0;

		_connection = new ConnectionSet( _serverSettings.maxConnections );

		// TODO: cozeroff
		/* 
		eventOvershootCounter = 0f;
		
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
		int portId = NetworkTransport.AddHost( topology, GetServerPort() );

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

	public EntityPrefabIndex FindEntityPrefabIndex( Entity prefab )
	{
		for( uint i = 0; i < _commonSettings.entityPrefab.Length; ++i )
		{
			if( _commonSettings.entityPrefab[i] == prefab )
			{
				return new EntityPrefabIndex(i);
			}
		}

		return new EntityPrefabIndex();
	}

	public Entity SpawnEntityServer( NodeIndex owner, EntityPrefabIndex prefabIndex, Vector3 position, Quaternion rotation, out string error )
	{
		if( owner != NodeIndex.SERVER_NODE_INDEX )
		{
			ClientConnection client = _connection.Get( owner );
			if( client == null )
			{
				error = $"Cannot spawn entity with ownerClientId {owner.GetClientIndex()}, client not yet connected!";
				return null;
			}
		}

		// Generate unique network id
		uint unique_id = _entityCounter;
		++_entityCounter;
		EntityIndex entityIndex = new EntityIndex( unique_id );

		// spawn
		Entity.Data data = new Entity.Data() { id = entityIndex, owner = owner, prefabIndex = prefabIndex };
		Entity.Spawn spawn = new Entity.Spawn( data, position, rotation );
		Entity entity = Entity.SpawnEntity( _commonSettings.entityPrefab, spawn, _localIndex );

		_entity.Add( entityIndex, entity );

		// notify all clients that an entity was created
		for( int i = 0; i < _connection.GetCount(); ++i )
		{
			string clientSendError;
			ClientConnection client = _connection.GetAt(i);
			using( BitWriter writer = GetPooledWriter() )
			{
				spawn.Write( writer );
				if( !SendInternal( client.GetId(), InternalMessage.EntityCreate, InternalChannel.Reliable, writer, false, out clientSendError ) )
				{
					error = $"Failed to send spawn to client {i}, error is:\n{clientSendError}\n";
					return null;
				}
			}
		}

		if( _onEntitySpawn != null ) { _onEntitySpawn.Invoke( entity ); }

		error = null;
		return entity;
	}

	// sends a custom message to the server via the default channel
	public bool SendCustomClient( NodeIndex client, BitWriter writer, bool sendImmediately, out string error )
	{
		return Send( client, InternalMessage.CustomClient, ChannelIndex.CreateInternal( InternalChannel.ClientReliable ), writer, sendImmediately, out error );
	}

	// sends a custom message to the server
	public bool SendCustomClient( NodeIndex client, uint customChannel, BitWriter writer, bool sendImmediately, out string error )
	{
		return Send( client, InternalMessage.CustomClient, ChannelIndex.CreateCustom( customChannel ), writer, sendImmediately, out error );
	}


	// PRIVATE

	bool SendInternal( NodeIndex client, InternalMessage message, InternalChannel channel, BitWriter writer, bool sendImmediately, out string error )
	{
		return Send( client, message, ChannelIndex.CreateInternal(channel), writer, sendImmediately, out error );
	}

	bool Send( NodeIndex client, InternalMessage message, ChannelIndex channel, BitWriter writer, bool sendImmediately, out string error )
	{
		int clientIndex = client.GetClientIndex();	
		byte[] key = _connection.GetAt(clientIndex-1).GetSharedSecretKey();
		// for the server, connectionId == clientIndex
		return base.Send( clientIndex, key, message, channel, writer, sendImmediately, out error );
	}

	ReceiveEvent HandleNetworkEvent()
	{
		ReceiveEvent e;
		using( BitReader reader = GetPooledReader() )
		{
			DataStream stream = reader.GetStream();

			int connectionId;
			ChannelIndex channel;
			e = PollReceive( stream, out connectionId, out channel );

			if( e == ReceiveEvent.Error || e == ReceiveEvent.NoMoreEvents ) { return e; }

			// on the server, the connectionId is one to one with the client index
			NodeIndex client = new NodeIndex( (uint)connectionId );

			switch( e )
			{
				case ReceiveEvent.Connect:
					//_profiler.RecordEvent(TickType.Receive, (uint)receivedSize, MessageManager.reverseChannels[channelId], "TRANSPORT_CONNECT");
					Log.Info( $"Client {client.GetClientIndex()} sent initial connection packet" );
					ClientConnection c;
					if( _commonSettings.enableEncryption )
					{
						// This client is required to complete the crypto-hail exchange.
						EllipticDiffieHellman keyExchange = SendCryptoHail();

						c = new ClientConnection( client, ClientConnection.Status.PendingConnectionChallengeResponse, keyExchange );
					}
					else
					{
						c = new ClientConnection( client, ClientConnection.Status.PendingConnectionRequest, null );		
					}
					_connection.Add( c.GetIndex(), c );
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
		}

		return e;
	}

	EllipticDiffieHellman SendCryptoHail()
	{
		// TODO: cozeroff

		return null;
	}

	void HandleMessage( BitReader reader, NodeIndex client, ChannelIndex channel )
	{
		InternalMessage messageType = UnwrapMessage( reader, client );
		if( messageType == InternalMessage.INVALID ) { return; }

		//_profiler.StartEvent(TickType.Receive, size, channelId, messageType);

		Log.Info( $"Handling message {AlpacaConstant.GetName(messageType)} from client {client.GetClientIndex()}" );

		ClientConnection connection = _connection.GetAt( client.GetClientIndex() - 1 );
		ClientConnection.Status state = connection.GetState();
		
		if( (state == ClientConnection.Status.PendingConnectionChallengeResponse) && (messageType != InternalMessage.ConnectionResponse ) )
		{
			Log.Error( $"Client {client.GetClientIndex()} is pending connection response, but client sent message {AlpacaConstant.GetName(messageType)} instead." );
			return;
		}

		if( (state == ClientConnection.Status.PendingConnectionRequest) && (messageType != InternalMessage.ConnectionRequest) )
		{
			Log.Error( $"Client {client.GetClientIndex()} is pending connection request, but client sent message {AlpacaConstant.GetName(messageType)} instead." );
			return;
		}

		switch( messageType )
		{
			case InternalMessage.ConnectionRequest:
				OnMessageConnectionRequest( client, reader );
				break;
			case InternalMessage.ConnectionResponse:
				// TODO: cozeroff crypto implementation
				Log.Error( "Crypto not implemented yet!" );
				break;
			case InternalMessage.CustomServer:
				OnMessageCustomServer( client, reader );
				break;
			default:
				Log.Error( $"Read unrecognized messageType{AlpacaConstant.GetName(messageType)}" );
				break;
		}

		//_profiler.EndEvent();
	}

	#region Message Handlers for specific InternalMessage types

	void OnMessageConnectionRequest( NodeIndex clientNode, BitReader reader )
	{
		// update ClientConnection state
		int clientIndex = clientNode.GetClientIndex();
		ClientConnection connection = _connection.GetAt( clientIndex - 1 );
		connection.SetConnected();

		// send the new client the data it needs, plus spawn instructions for all current entities
		using( BitWriter writer = GetPooledWriter() )
		{
			writer.Packed<Int32>( clientIndex  );
			writer.Packed<float>( _networkTime );
			writer.Packed<Int32>( NetworkTransport.GetNetworkTimestamp() );

			int entityCount = _entity.GetCount();
			writer.Packed<Int32>( entityCount );
			
			Entity e;
			for( int i = 0; i < entityCount; ++i )
			{
				e = _entity.GetAt(i);
				e.MakeSpawn().Write( writer );
			}

			string error;
			if( !SendInternal( clientNode, InternalMessage.ConnectionApproved, InternalChannel.Reliable, writer, true, out error ) )
			{
				Log.Error( $"OnMessageConnectionRequest failed to send connection approval data to new client {clientIndex}.\n{error}" );
			}
		}

		// Inform old clients of the new client
		using( BitWriter writer = GetPooledWriter() )
		{
			writer.Packed<Int32>( clientIndex );

			for( int i = 0; i < _connection.GetCount(); ++i )
			{
				if( i == (clientIndex - 1) ) { continue; } // skip the new client

				ClientConnection sibling = _connection.GetAt(i);
				string error;
				if( !SendInternal( sibling.GetId(), InternalMessage.SiblingConnected, InternalChannel.Reliable, writer, false, out error ) )
				{
					Log.Error( $"OnMessageConnectionRequest failed to send SiblingConnected message to all other clients.\n{error}" );
				}
			}
		}

		// callback
		//if( _onClientConnect != null ) { _onClientConnect.Invoke( clientNode ); }
	}

	void OnMessageCustomServer( NodeIndex clientNode, BitReader reader )
	{
		if( _onMessageCustomServer != null )
		{
			_onMessageCustomServer.Invoke( clientNode, reader );
		}
		else
		{
			Log.Error( $"Received custom message from client {clientNode.GetClientIndex()} but no custom handler is set" ); 
		}
	}

	#endregion // InternalMessage Handlers
}

} // namespace Alpaca