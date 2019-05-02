	/*
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

		public int GetRemoteDelayTimeMS(uint clientId, int remoteTimestamp, out byte error)
		{
			if (netId.IsServer())
			{
				netId.ConnectionId = (ushort)serverConnectionId;
				netId.HostId = (byte)serverHostId;
			}
			return NetworkTransport.GetRemoteDelayTimeMS(netId.HostId, netId.ConnectionId, remoteTimestamp, out error);
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
	}
	*/