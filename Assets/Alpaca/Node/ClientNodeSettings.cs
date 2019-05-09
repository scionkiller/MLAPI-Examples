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

public class ClientNodeSettings : MonoBehaviour
{
	// how to contact the server to connect
	public string connectAddress = "127.0.0.1";
}

// ignore Obsolete warning for UNET
#pragma warning disable 618

public class ClientNode : CommonNode
{
	#region INTERNAL CLASSES

	// this is the mirror status to ServerNode.ClientConnection.Status
	private enum Status
	{
		  NotStarted                     // NetworkTransport not initialized
		, Disconnected                   // not yet connected to server or lost connection
		, WaitingForConnectEvent         // waiting for the NetworkTransport connect event
		, WaitingForChallenge            // waiting for cryptography challenge from server (Alpaca message)
		, WaitingForConnectionApproval   // waiting for final connection approval from server (Alpaca message)
		, Connected                      // Note that because we use UDP, we never truly have a live connection to the server even in this state.
	}

	// for now, we only store the peers id, which makes this class actually redundant,
	// we could just use a HashSet instead. But we retain this for now until we are
	// sure we don't need more per-Peer data
	private class Peer
	{
		NodeIndex _id;
	}

	private class PeerSet : ArraySet<NodeIndex, Peer>
	{
		public PeerSet( int capacity ) : base(capacity) {}
	}

	#endregion // INTERNAL CLASSES


	ClientNodeSettings _clientSettings;
	Status _status;

	PeerSet _peer;

	byte[] _serverKey;
	
	Action _onDisconnect = null;
	Action<BitReader> _onMessageCustomClient = null;
	

	public void SetOnDisconnect         ( Action            callback ) { _onDisconnect       = callback; }
	public void SetOnMessageCustomClient( Action<BitReader> callback ) { _onMessageCustomClient = callback; }

	public bool   IsRunning()            { return _status != Status.NotStarted; }
	public bool   IsConnectedToServer()  { return _status == Status.Connected; }
	public string GetConnectionAddress() { return _clientSettings.connectAddress; }

	public ClientNode( CommonNodeSettings commonSettings, ClientNodeSettings clientSettings ) : base( commonSettings )
	{
		_clientSettings = clientSettings;
		_localIndex = new NodeIndex(); // ensure this is invalid
		_status = Status.NotStarted;
	}

	public override bool Start( out string error )
	{
		ConnectionConfig config = InitializeNetwork( out error );
		if( config == null )
		{
			return false;
		}

		// clients only ever have one connection, to the server
		HostTopology topology = new HostTopology( config, 1 );
		// 0 in AddHost means we choose a random free port for the client
		int hostId = NetworkTransport.AddHost( topology, 0 );
		if( hostId != 0 )
		{
			error = $"Got unexpected hostId:{hostId}";
			return false;
		}

		_status = Status.Disconnected;

		return Reconnect( out error );
	}

	public bool Reconnect( out string error )
	{	
		byte errorByte;
		NetworkTransport.Connect( 0, GetConnectionAddress(), GetServerPort(), 0, out errorByte );

		if( (NetworkError)errorByte != NetworkError.Ok )
		{
			error = StringFromError( errorByte );
			return false;
		}

		_status = Status.WaitingForConnectEvent;
		error = string.Empty;
		return true;
	}

	public void UpdateClient()
	{
		Debug.Assert( IsRunning() );

		// Phase 1: Have conducts update their SyncVars
		// TODO: cozeroff
		// foreach Type t in _conductTypes
		// 	foreach Conduct c in _ownedConducts[t]
		// 		bool sendNeeded = c.LocalAct();
		// 		if( sendNeeded )
		// 		{
		// 			c.Write( NodeIndex.SERVER_NODE_INDEX )
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


	// PRIVATE

	bool Send( InternalMessage message, InternalChannel channel, BitWriter writer, bool sendImmediately, out string error )
	{
		ChannelIndex channelIndex = ChannelIndex.CreateInternal(channel);
		// for the client, connectionId == 1 always (server)
		return base.Send( 1, _serverKey, message, channelIndex, writer, sendImmediately, out error );
	}

	void SendConnectionRequest()
	{
		// send the new client the data it needs, plus spawn instructions for all current entities
		using( BitWriter writer = GetPooledWriter() )
		{
			string error;
			if( !Send( InternalMessage.ConnectionRequest, InternalChannel.Reliable, writer, true, out error ) )
			{
				Log.Error( $"Failed to send ConnectionRequest to server.\n{error}" );
			}
		}
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
			Debug.Assert( connectionId == 1 ); // we only have a connection to the server

			switch( e )
			{
				case ReceiveEvent.Connect:
					Log.Info( "Connect Event From Server." );
					if( _status == Status.WaitingForConnectEvent )
					{
						if( _commonSettings.enableEncryption )
						{
							_status = Status.WaitingForChallenge;
						}
						else
						{
							SendConnectionRequest();
							_status = Status.WaitingForConnectionApproval;
						}
					}
					else
					{
						Log.Warn( $"Received Connect event when we weren't expecting it. We are in state: { _status }" );
					}
					break;

				case ReceiveEvent.Message:
					HandleMessage( reader, channel );
					break;
					
				case ReceiveEvent.Disconnect:
					// clear our local index, indicating that connection was lost
					_localIndex = new NodeIndex();
					// call disconnect callback if any, otherwise log error
					if( _onDisconnect != null )
					{
						_onDisconnect.Invoke();
					}
					else
					{
						Log.Error( "Disconnected from server " );
					}
					break;
			}
		}

		return e;
	}

	void HandleMessage( BitReader reader, ChannelIndex channel )
	{
		InternalMessage messageType = UnwrapMessage( reader, NodeIndex.SERVER_NODE_INDEX );
		if( messageType == InternalMessage.INVALID ) { return; }

		//_profiler.StartEvent(TickType.Receive, size, channelId, messageType);

		Log.Info( $"Handling message {AlpacaConstant.GetName(messageType)} from server" );

		if( (_status == Status.WaitingForChallenge) && (messageType != InternalMessage.ConnectionChallenge) )
		{
			Log.Error( $"We are waiting for challenge, but server sent message {AlpacaConstant.GetName(messageType)} instead." );
			return;
		}

		if( (_status == Status.WaitingForConnectionApproval) && (messageType != InternalMessage.ConnectionApproved ) )
		{
			Log.Error( $"We are waiting for connection approval, but server sent message {AlpacaConstant.GetName(messageType)} instead." );
			return;
		}

		switch( messageType )
		{
			case InternalMessage.ConnectionChallenge:
				// TODO: cozeroff crypto implementation
				Log.Error( "Crypto not implemented yet!" );
				break;
			case InternalMessage.ConnectionApproved:
				OnMessageConnectionApproved( reader );
				break;

			// TODO: cozeroff handle sibling connect/disconnect
			
			case InternalMessage.CustomClient:
				OnMessageCustomClient( reader );
				break;
			default:
				Log.Error( $"Read unrecognized messageType{AlpacaConstant.GetName(messageType)}" );
				break;
		}

		//_profiler.EndEvent();
	}


	#region Message Handlers for specific InternalMessage types

	void OnMessageConnectionApproved( BitReader reader )
	{
		int ourIndex = reader.Packed<Int32>();
		_localIndex = new NodeIndex( (uint)ourIndex );

		_networkTime = reader.Packed<float>();

		// TODO: cozeroff
		// not sure what we do with the network timestamp for now
		int networkTimestamp = reader.Packed<Int32>();

		// spawn entities already existing in the world
		int entityCount = reader.Packed<Int32>();
		
		Entity.Spawn s = new Entity.Spawn();
		for( int i = 0; i < entityCount; ++i )
		{
			s.Read( reader );
			// TODO: cozeroff spawn the entity here 
		}

		_status = Status.Connected;
	}

	void OnMessageCustomClient( BitReader reader )
	{
		if( _onMessageCustomClient != null )
		{
			_onMessageCustomClient.Invoke( reader );
		}
		else
		{
			Log.Error( $"Received custom message from server but no custom handler is set" ); 
		}
	}

	#endregion // InternalMessage Handlers
}

#pragma warning restore 618

} // namespace Alpaca