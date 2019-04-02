using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Networking;

using Alpaca.Serialization;


namespace Alpaca
{

public class ServerNodeSettings : MonoBehaviour
{
	public int clientHandshakeTimeout = 10;
}


// ignore Obsolete warning for UNET
#pragma warning disable 618

public class ServerNode : CommonNode
{
	ServerNodeSettings _serverSettings;
	bool _isRunning;

	// simple counters used to make sure that EntityId and ConductIds are kept unique
	uint _entityIdCounter;
	uint _componentIdCounter;




	// TODO: cozeroff
	private float lastReceiveTickTime;
	private float lastSendTickTime;
	private float lastEventTickTime;
	private float eventOvershootCounter;
	private float lastTimeSyncTime;

	ClientSet _connectedClients;
	PendingClientSet _pendingClients;

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

		_entityIdCounter = 0;
		_componentIdCounter = 0;

		// TODO: cozeroff









		/* 
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

		// TODO: cozeroff topology here

		_isRunning = true;
		return _isRunning;
	}

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
}



} // namespace Alpaca