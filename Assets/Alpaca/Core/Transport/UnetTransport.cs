using System.Collections.Generic;

using UnityEngine.Networking;


// ignore Obsolete warning for UNET
#pragma warning disable 618

namespace Alpaca.Transport
{


public abstract class UnetTransport
{
	
}

public class ClientTransport : UnetTransport
{

}

public class ServerTransport : UnetTransport
{
	/*
	// returns true on success, false on failure
	// on the server, we expect to 
	public bool Start( string address, int port, out string error)
	{
		NetworkTransport.Init();
		int hostId = NetworkTransport.AddHost( topology );
		byte errorByte;
		int connectionId = NetworkTransport.Connect(hostId, address, port, 0, out errorByte );
		error = StringFromError( errorByte );

		return error == string.Empty;
	}

	*/



}


} // namespace Alpaca.Transport

#pragma warning restore 618


	/*
		public ChannelType GetInternalChannel() { return ChannelType.ReliableFragmentedSequenced; }

        public static readonly List<TransportHost> ServerTransports = new List<TransportHost>()
        {
            new TransportHost()
            {
                Name = "UDP Socket",
                Port = 7777,
            }
        };

        

		public void RegisterServerListenSocket( object settings )
        {
            HostTopology topology = new HostTopology((ConnectionConfig)settings, AlpacaNetwork.GetSingleton().config.MaxConnections);
            for( int i = 0; i < ServerTransports.Count; i++ )
            {
                NetworkTransport.AddHost(topology, ServerTransports[i].Port);
            }
        }

        public void DisconnectClientFromServer( NodeId id)
        {
			Debug.Assert( !id.IsServer() );
            NetworkTransport.Disconnect( id.GetHostId(), id.GetConnectionId(), out byte error);
        }

		public void ConnectToServerFromClient( string address, int port, object settings, out byte error )
        {
            _serverHostId = NetworkTransport.AddHost( new HostTopology( (ConnectionConfig)settings, 1 ) );
            _serverConnectionId = NetworkTransport.Connect( _serverHostId, address, port, 0, out error);
        }

        public void DisconnectServerFromClient()
		{
			NetworkTransport.Disconnect( _serverHostId, _serverConnectionId, out byte error);
		}

        public int GetCurrentRoundTripTime( NodeId id, out byte error)
        {
			if( id.IsServer() )
			{
				id.ConnectionId = (ushort)serverConnectionId;
				id.HostId = (byte)serverHostId;
			}
            return NetworkTransport.GetCurrentRTT( id.GetHostId(), id.GetConnectionId(), out error);
        }

        public int GetNetworkTimestamp() => NetworkTransport.GetNetworkTimestamp();

        public int GetRemoteDelayTimeMS(uint clientId, int remoteTimestamp, out byte error)
        {
			if (netId.IsServer())
            {
                netId.ConnectionId = (ushort)serverConnectionId;
                netId.HostId = (byte)serverHostId;
            }
            return NetworkTransport.GetRemoteDelayTimeMS(netId.HostId, netId.ConnectionId, remoteTimestamp, out error);
        }

        public NetEventType PollReceive(out uint clientId, out int channelId, ref byte[] data, int bufferSize, out int receivedSize, out byte error)
        {
            NetworkEventType eventType = NetworkTransport.Receive(out int hostId, out int connectionId, out channelId, data, bufferSize, out receivedSize, out byte err);
            clientId = new NetId((byte)hostId, (ushort)connectionId, false).GetClientId();
            NetworkError errorType = (NetworkError)err;
            if (errorType == NetworkError.Timeout)
                eventType = NetworkEventType.DisconnectEvent; //In UNET. Timeouts are not disconnects. We have to translate that here.
            error = 0;

            //Translate NetworkEventType to NetEventType
            switch (eventType)
            {
                case NetworkEventType.DataEvent:
                    return NetEventType.Data;
                case NetworkEventType.ConnectEvent:
                    return NetEventType.Connect;
                case NetworkEventType.DisconnectEvent:
                    return NetEventType.Disconnect;
                case NetworkEventType.Nothing:
                    return NetEventType.Nothing;
                case NetworkEventType.BroadcastEvent:
                    return NetEventType.Nothing;
            }
            return NetEventType.Nothing;
        }

        public void QueueMessageForSending( NodeId id, byte[] dataBuffer, int dataSize, int channelId, bool skipqueue, out byte error)
        {
			if( id.IsServer() )
            {
                netId.ConnectionId = (ushort)serverConnectionId;
                netId.HostId = (byte)serverHostId;
            }

            if (skipqueue)
                NetworkTransport.Send(netId.HostId, netId.ConnectionId, channelId, dataBuffer, dataSize, out error);
            else
                NetworkTransport.QueueMessageForSending(netId.HostId, netId.ConnectionId, channelId, dataBuffer, dataSize, out error);
        }

        public void Shutdown()
		{
			NetworkTransport.Shutdown();
		}

        public void SendQueue( NodeId id, out byte error)
        {
			if( id.IsServer() )
            {
                netId.ConnectionId = (ushort)serverConnectionId;
                netId.HostId = (byte)serverHostId;
            }
            NetworkTransport.SendQueuedMessages(netId.HostId, netId.ConnectionId, out error);
        }

        public int AddChannel(ChannelType type, object settings)
        {
            ConnectionConfig config = (ConnectionConfig)settings;
            switch (type)
            {
                case ChannelType.Unreliable:
                    return config.AddChannel(QosType.Unreliable);
                case ChannelType.UnreliableFragmented:
                    return config.AddChannel(QosType.UnreliableFragmented);
                case ChannelType.UnreliableSequenced:
                    return config.AddChannel(QosType.UnreliableSequenced);
                case ChannelType.Reliable:
                    return config.AddChannel(QosType.Reliable);
                case ChannelType.ReliableFragmented:
                    return config.AddChannel(QosType.ReliableFragmented);
                case ChannelType.ReliableSequenced:
                    return config.AddChannel(QosType.ReliableSequenced);
                case ChannelType.StateUpdate:
                    return config.AddChannel(QosType.StateUpdate);
                case ChannelType.ReliableStateUpdate:
                    return config.AddChannel(QosType.ReliableStateUpdate);
                case ChannelType.AllCostDelivery:
                    return config.AddChannel(QosType.AllCostDelivery);
                case ChannelType.UnreliableFragmentedSequenced:
                    return config.AddChannel(QosType.UnreliableFragmentedSequenced);
                case ChannelType.ReliableFragmentedSequenced:
                    return config.AddChannel(QosType.ReliableFragmentedSequenced);
            }
            return 0;
        }

        public object GetSettings()
        {
            NetworkTransport.Init();
            return new ConnectionConfig()
            {
                SendDelay = 0
            };
        }
    }
	*/