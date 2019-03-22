using UnityEngine;


namespace Alpaca
{

public class ServerNodeSettings : MonoBehaviour
{
	public int clientHandshakeTimeout = 10;
}

public class ServerNode
{
	CommonNodeSettings _commonSettings;
	ServerNodeSettings _serverSettings;


	// A synchronized time, represents the time in seconds since the server started. Replicated across all nodes.
	float _networkTime;

	// storage for a single message of max size
	byte[] _messageBuffer = null;

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
	System.Action<NodeId> _onClientConnect = null;
	System.Action<NodeId> _onClientDisconnect = null;
	System.Action<Entity> _onAvatarSpawn = null;
	System.Action<Entity> _onEntitySpawn = null;
	// TODO: cozeroff probably not a stream, should be a BitReader or similar
	System.Action<NodeId, Stream> _onCustomMessage = null;







	public NodeId GetLocalNodeId() { return NodeId.SERVER_NODE_ID; }
	public float GetNetworkTime() { return _networkTime; }
	
	public void SetOnClientConnect   ( System.Action<NodeId> callback         ) { _onClientConnect    = callback; }
	public void SetOnClientDisconnect( System.Action<NodeId> callback         ) { _onClientDisconnect = callback; }
	public void SetOnAvatarSpawn     ( System.Action<Entity> callback         ) { _onAvatarSpawn      = callback; }
	public void SetOnEntitySpawn     ( System.Action<Entity> callback         ) { _onEntitySpawn      = callback; }
	public void SetOnCustomMessage   ( System.Action<NodeId, Stream> callback ) { _onCustomMessage    = callback; }


	public ServerNode( CommonNodeSettings commonSettings, ServerNodeSettings serverSettings )
	{
		_commonSettings = commonSettings;
		_serverSettings = serverSettings;

		_networkTime = 0f;

		_messageBuffer = new byte[_commonSettings.messageBufferSize];

		_entityIdCounter = 0;
		_componentIdCounter = 0;

		// TODO: cozeroff

		Log.LogInfo( "ServerNode ctor") ;


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
}

} // namespace Alpaca