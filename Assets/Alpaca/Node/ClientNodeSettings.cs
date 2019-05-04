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
	bool _isRunning;

	PeerSet _peer;

	byte[] _serverKey;


	public bool IsRunning() { return _isRunning; }
	public bool IsConnectedToServer() { return _localIndex.IsValidClientIndex(); }
	public string GetConnectionAddress() { return _clientSettings.connectAddress; }

	public ClientNode( CommonNodeSettings commonSettings, ClientNodeSettings clientSettings ) : base( commonSettings )
	{
		_clientSettings = clientSettings;
		_isRunning = false;
	}

	public override bool Start( out string error )
	{
		// note that on the client, we don't actually use the config that is returned, but we still need to call InitializeNetwork
		ConnectionConfig config = InitializeNetwork( out error );
		if( config == null )
		{
			return false;
		}
		
		byte errorByte;
		NetworkTransport.Connect( 0, GetConnectionAddress(), GetConnectionPort(), 0, out errorByte );

		if( (NetworkError)errorByte != NetworkError.Ok )
		{
			error = StringFromError( errorByte );
			return false;
		}

		_isRunning = true;
		return _isRunning;
	}

	public void UpdateClient()
	{
		Debug.Assert( _isRunning );

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

	bool Send( NodeIndex client, InternalMessage message, InternalChannel channel, BitWriter writer, bool sendImmediately, out string error )
	{
		ChannelIndex channelIndex = ChannelIndex.CreateInternal(channel);
		// for the client, connectionId == 0 always (server)
		return base.Send( 0, _serverKey, message, channelIndex, writer, sendImmediately, out error );
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

			if( e == ReceiveEvent.Error ) { return e; }
			Debug.Assert( connectionId == 0 ); // we only have a connection to the server

			switch( e )
			{
				case ReceiveEvent.Connect:
					Log.Info( "Connect Event From Server." );
					// TODO: cozeroff not sure what to do here, does the client get these events?
					break;

				case ReceiveEvent.Message:
					HandleMessage( reader, channel );
					break;
					
				case ReceiveEvent.Disconnect:
					Log.Info( "Disconnect Event From Server." );
					// TODO: cozeroff not sure what to do here, does the client get these events?
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

		// TODO: cozeroff we could do something similar for our internal state		
		/*if( (state == ClientConnection.State.PendingConnectionRequest) && (messageType != InternalMessage.ConnectionRequest) )
		{
			Log.Error( $"Client {client.GetClientIndex()} is pending connection request, but client sent message {AlpacaConstant.GetName(messageType)} instead." );
			return;
		}

		if( (state == ClientConnection.State.PendingConnectionResponse) && (messageType != InternalMessage.ConnectionResponse ) )
		{
			Log.Error( $"Client {client.GetClientIndex()} is pending connection response, but client sent message {AlpacaConstant.GetName(messageType)} instead." );
			return;
		}*/

		switch( messageType )
		{
			default:
				Log.Error( $"Read unrecognized messageType{AlpacaConstant.GetName(messageType)}" );
				break;
		}

		//_profiler.EndEvent();
	}
}

#pragma warning restore 618

} // namespace Alpaca