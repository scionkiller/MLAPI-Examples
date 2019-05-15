/*
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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
}

} // namespace Alpaca
*/