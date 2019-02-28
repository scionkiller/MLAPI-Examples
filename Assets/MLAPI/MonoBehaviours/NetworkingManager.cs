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

using Alpaca.Logging;
using Alpaca.Components;
using Alpaca.Configuration;
#if !DISABLE_CRYPTOGRAPHY
using Alpaca.Cryptography;
#endif
using Alpaca.Data;
using Alpaca.Internal;
using Alpaca.Profiling;
using Alpaca.Serialization;
using Alpaca.Transports;
using Alpaca.Transports.UNET;
using BitStream = Alpaca.Serialization.BitStream;


namespace Alpaca
{
	public class ClientSet : ArraySet<uint, NetworkedClient>
	{
		public ClientSet( int capacity ) : base(capacity)
		{}
	}

	public class PendingClientSet : ArraySet<uint, PendingClient>
	{
		public PendingClientSet( int capacity ) : base(capacity)
		{}
	}

    [AddComponentMenu("Alpaca/NetworkingManager", -100)]
    public class NetworkingManager : MonoBehaviour
    {
		static NetworkingManager _singleton;
		public static NetworkingManager GetSingleton() { return _singleton; }


        /// <summary>
        /// A synchronized time, represents the time in seconds since the server application started. Is replicated across all clients
        /// </summary>
        public float NetworkTime { get; internal set; }

        [HideInInspector]
        public LogLevel LogLevel = LogLevel.Normal;
        
        /// <summary>
        /// Gets the networkId of the server
        /// </summary>
		public uint ServerClientId => config.NetworkTransport != null ? config.NetworkTransport.ServerClientId : 0;

        /// <summary>
        /// The clientId the server calls the local client by, only valid for clients
        /// </summary>
        public uint LocalClientId 
        {
            get
            {
                if (IsServer) return config.NetworkTransport.ServerClientId;
				else return localClientId;
            }
            internal set
            {
                localClientId = value;
            }
        }
        private uint localClientId;

		// TODO: make these private and improve encapsulation
		// Will require moving of lots of code that belongs in this file but is scattered around
        public ClientSet _connectedClients;
        public PendingClientSet _pendingClients;
 
        public bool IsServer { get; internal set; }

        public bool IsClient { get; internal set; }

        public bool IsHost => IsServer && IsClient;

        public bool IsListening { get; internal set; }
        private byte[] messageBuffer;

        public bool IsConnectedClient { get; internal set; }

        public Action<uint> OnClientConnectedCallback = null;
        public Action<uint> OnClientDisconnectCallback = null;
		public Action<GameObject> OnAvatarSpawn = null;
		public Action<GameObject> OnObjectSpawn = null;
		
        /// <summary>
        /// Delegate type called when connection has been approved
        /// </summary>
        /// <param name="clientId">The clientId of the approved client</param>
        /// <param name="approved">Wheter or not the client was approved</param>
        public delegate void ConnectionApprovedDelegate(uint clientId, bool approved);

        /// <summary>
        /// The callback to invoke during connection approval
        /// </summary>
        public Action<byte[], uint, ConnectionApprovedDelegate> ConnectionApprovalCallback = null;

        public NetworkConfig config;

        /// <summary>
        /// Delegate used for incoming custom messages
        /// </summary>
        /// <param name="clientId">The clientId that sent the message</param>
        /// <param name="stream">The stream containing the message data</param>
        public delegate void CustomMessageDelegete(uint clientId, Stream stream);
        /// <summary>
        /// Event invoked when custom messages arrive
        /// </summary>
        public event CustomMessageDelegete OnIncomingCustomMessage;
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
                if (LogHelper.CurrentLogLevel <= LogLevel.Error) LogHelper.LogWarning("Only the server can broadcast custom messages");
                return;
            }
			for (int i = 0; i < _connectedClients.GetCount(); ++i)
			{
				InternalMessageHandler.Send( _connectedClients.GetAt(i).ClientId, Constants.ALPACA_CUSTOM_MESSAGE, string.IsNullOrEmpty(channel) ? "ALPACA_DEFAULT_MESSAGE" : channel, stream, security);
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
            InternalMessageHandler.Send(clientId, Constants.ALPACA_CUSTOM_MESSAGE, string.IsNullOrEmpty(channel) ? "ALPACA_DEFAULT_MESSAGE" : channel, stream, security);
        }

		// get the public key we need for encrypting a message to the target client, or server if we are a client
		public byte[] GetPublicEncryptionKey( uint clientId )
		{
			if( IsServer )
			{
				NetworkedClient c = _connectedClients.Get( clientId );
				if( c != null ) { return c.AesKey; }
				
				PendingClient p = _pendingClients.Get( clientId );
				if( p != null ) { return p.AesKey; }

				return null;
			}
			else
			{
				return clientAesKey;
			}
		}

        private object Init(bool server)
        {
            if (LogHelper.CurrentLogLevel <= LogLevel.Developer) LogHelper.LogInfo("Init()");
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
            SpawnManager.releasedNetworkObjectIds.Clear();

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
            else if (config.Transport == DefaultTransport.ALPACA_Relay)
                config.NetworkTransport = new RelayedTransport();
            else if (config.Transport == DefaultTransport.Custom && config.NetworkTransport == null)
                throw new NullReferenceException("The current NetworkTransport is null");

            object settings = config.NetworkTransport.GetSettings(); //Gets a new "settings" object for the transport currently used.

			HashSet<string> networkedPrefabName = new HashSet<string>();
			for (int i = 0; i < config.NetworkedPrefabs.Count; i++)
			{
				if (networkedPrefabName.Contains(config.NetworkedPrefabs[i].name))
				{
					if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Duplicate NetworkedPrefabName " + config.NetworkedPrefabs[i].name);
					continue;
				}
				networkedPrefabName.Add(config.NetworkedPrefabs[i].name);
			}

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

		// Returns true on success
        public bool StartServer( out string errorString )
        {
            if (LogHelper.CurrentLogLevel <= LogLevel.Developer) LogHelper.LogInfo("StartServer()");
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
            if (LogHelper.CurrentLogLevel <= LogLevel.Developer) LogHelper.LogInfo("StopServer()");

            // Don't know if I have to disconnect the clients. I'm assuming the NetworkTransport does all the cleaning on shutdown.
			// But this way the clients get a disconnect message from server (so long it does't get lost)
			for( int i = 0; i < _connectedClients.GetCount(); ++i )
			{
				NetworkedClient c = _connectedClients.GetAt(i);
				if( c.ClientId != config.NetworkTransport.ServerClientId )
				{
					config.NetworkTransport.DisconnectClient( c.ClientId );
				}
			}
			for( int i = 0; i < _pendingClients.GetCount(); ++i )
			{
				PendingClient c = _pendingClients.GetAt(i);
				if( c.ClientId != config.NetworkTransport.ServerClientId )
				{
					config.NetworkTransport.DisconnectClient( c.ClientId );
				}
			}
            
            IsServer = false;
            Shutdown();
        }

		// Returns true on success
        public bool StartClient( out string errorString )
        {
            if (LogHelper.CurrentLogLevel <= LogLevel.Developer) LogHelper.LogInfo("StartClient()");
			
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
            if (LogHelper.CurrentLogLevel <= LogLevel.Developer) LogHelper.LogInfo("StopClient()");
            IsClient = false;
            config.NetworkTransport.DisconnectFromServer();
            IsConnectedClient = false;
            Shutdown();
        }

        public void StartHost(Vector3? pos = null, Quaternion? rot = null, int prefabId = -1)
        {
            if (LogHelper.CurrentLogLevel <= LogLevel.Developer) LogHelper.LogInfo("StartHost()");
            if (IsServer || IsClient)
            {
                if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Cannot start host while an instance is already running");
                return;
            }

            if (config.ConnectionApproval)
            {
                if (ConnectionApprovalCallback == null)
                {
                    if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("No ConnectionApproval callback defined. Connection approval will timeout");
                }
            }
            object settings = Init(true);
            config.NetworkTransport.RegisterServerListenSocket(settings);

            IsServer = true;
            IsClient = true;
            IsListening = true;

			uint hostClientId = config.NetworkTransport.ServerClientId;
            _connectedClients.Add( hostClientId, new NetworkedClient()
            {
                ClientId = hostClientId
            });

            if( prefabId != -1)
            {
                SpawnManager.CreateSpawnedObject(prefabId, 0, hostClientId, true, pos.GetValueOrDefault(), rot.GetValueOrDefault(), true, null, false, 0, false);
            }

            //SpawnSceneObjects();
        }

        public void StopHost()
        {
            if (LogHelper.CurrentLogLevel <= LogLevel.Developer) LogHelper.LogInfo("StopHost()");
            IsServer = false;
            IsClient = false;
            StopServer();
            //We don't stop client since we dont actually have a transport connection to our own server
        }

        private void OnEnable()
        {
            if( _singleton != null )
            {
				Debug.LogError( "Error: NetworkingManager already exists." );
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
            if (LogHelper.CurrentLogLevel <= LogLevel.Developer) LogHelper.LogInfo("Shutdown()");
            NetworkProfiler.Stop();
            IsListening = false;
            IsServer = false;
            IsClient = false;
            SpawnManager.DestroyNonSceneObjects();

            if (config != null && config.NetworkTransport != null) //The Transport is set during Init time, thus it is possible for the Transport to be null
                config.NetworkTransport.Shutdown();
        }

        private float lastReceiveTickTime;
        private float lastSendTickTime;
        private float lastEventTickTime;
        private float eventOvershootCounter;
        private float lastTimeSyncTime;
        private void Update()
        {
            if(IsListening)
            {
                if((NetworkTime - lastSendTickTime >= (1f / config.SendTickrate)) || config.SendTickrate <= 0)
                {
                    NetworkedObject.NetworkedVarPrepareSend();
					for( int i = 0; i < _connectedClients.GetCount(); ++i )
					{
						uint clientId = _connectedClients.GetAt(i).ClientId;
                        byte error;
                        config.NetworkTransport.SendQueue(clientId, out error);
                        if (LogHelper.CurrentLogLevel <= LogLevel.Developer) LogHelper.LogInfo("Send Pending Queue: " + clientId);
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
                                    if (LogHelper.CurrentLogLevel <= LogLevel.Developer) LogHelper.LogInfo("Client Connected");
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
                                            InternalMessageHandler.Send(clientId, Constants.ALPACA_CERTIFICATE_HAIL, "ALPACA_INTERNAL", hailStream, SecuritySendFlags.None, true);
                                        }
                                    }
                                    else
                                    {
#endif
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
                                    if (LogHelper.CurrentLogLevel <= LogLevel.Developer) LogHelper.LogInfo("Connected");
                                    if (!config.EnableEncryption) SendConnectionRequest();
                                    StartCoroutine(ApprovalTimeout(clientId));
                                }
                                NetworkProfiler.EndEvent();
                                break;
                            case NetEventType.Data:
                                if (LogHelper.CurrentLogLevel <= LogLevel.Developer) LogHelper.LogInfo($"Incoming Data From {clientId} : {receivedSize} bytes");

                                HandleIncomingData(clientId, data, channelId, receivedSize);
                                break;
                            case NetEventType.Disconnect:
                                NetworkProfiler.StartEvent(TickType.Receive, 0, "NONE", "TRANSPORT_DISCONNECT");
                                if (LogHelper.CurrentLogLevel <= LogLevel.Developer) LogHelper.LogInfo("Disconnect Event From " + clientId);

                                if (IsServer)
								{
                                    OnClientDisconnect(clientId);
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
                    writer.WriteUInt64Packed(config.GetConfig());

                    if( config.ConnectionApproval )
					{
                    	writer.WriteByteArray(config.ConnectionData);
					}
                }

                InternalMessageHandler.Send(ServerClientId, Constants.ALPACA_CONNECTION_REQUEST, "ALPACA_INTERNAL", stream, SecuritySendFlags.Authenticated | SecuritySendFlags.Encrypted, true);
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
                if (LogHelper.CurrentLogLevel <= LogLevel.Developer) LogHelper.LogInfo("Client " + clientId + " Handshake Timed Out");
                DisconnectClient(clientId);
            }
        }

        private void HandleIncomingData(uint clientId, byte[] data, int channelId, int totalSize)
        {
            if (LogHelper.CurrentLogLevel <= LogLevel.Developer) LogHelper.LogInfo("Unwrapping Data Header");

            using (BitStream inputStream = new BitStream(data))
            {
                inputStream.SetLength(totalSize);
                
                using (BitStream messageStream = MessageManager.UnwrapMessage(inputStream, clientId, out byte messageType, out SecuritySendFlags security))
                {
                    if (messageStream == null)
                    {
                        if (LogHelper.CurrentLogLevel <= LogLevel.Error) LogHelper.LogError("Message unwrap could not be completed. Was the header corrupt? Crypto error?");
                        return;
                    }
                    else if (messageType == Constants.INVALID)
                    {
                        if (LogHelper.CurrentLogLevel <= LogLevel.Error) LogHelper.LogError("Message unwrap read an invalid messageType");
                        return;
                    }

                    uint headerByteSize = (uint)Arithmetic.VarIntSize(messageType);
                    NetworkProfiler.StartEvent(TickType.Receive, (uint)(totalSize - headerByteSize), channelId, messageType);

                    if (LogHelper.CurrentLogLevel <= LogLevel.Developer) LogHelper.LogInfo("Data Header: messageType=" + messageType);

                    // Pending client tried to send a network message that was not the connection request before he was accepted.
					if( IsServer )
					{
						PendingClient p = _pendingClients.Get(clientId);
						if(  (p.ConnectionState == PendingClient.State.PendingHail       && messageType != Constants.ALPACA_CERTIFICATE_HAIL_RESPONSE)
						  || (p.ConnectionState == PendingClient.State.PendingConnection && messageType != Constants.ALPACA_CONNECTION_REQUEST       )
						  )
						{
							if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Message recieved from clientId " + clientId + " before it has been accepted");
							return;
						}
					}

                    #region INTERNAL MESSAGE

                    switch (messageType)
                    {
                        case Constants.ALPACA_CONNECTION_REQUEST:
                            if (IsServer)
                                InternalMessageHandler.HandleConnectionRequest(clientId, messageStream, channelId);
                            break;
                        case Constants.ALPACA_CONNECTION_APPROVED:
                            if (IsClient)
                                InternalMessageHandler.HandleConnectionApproved(clientId, messageStream, channelId);
                            break;
                        case Constants.ALPACA_ADD_OBJECT:
                            if (IsClient) InternalMessageHandler.HandleAddObject(clientId, messageStream, channelId);
                            break;
                        case Constants.ALPACA_CLIENT_DISCONNECT:
                            if (IsClient)
                                InternalMessageHandler.HandleClientDisconnect(clientId, messageStream, channelId);
                            break;
                        case Constants.ALPACA_DESTROY_OBJECT:
                            if (IsClient) InternalMessageHandler.HandleDestroyObject(clientId, messageStream, channelId);
                            break;
                        case Constants.ALPACA_SPAWN_POOL_OBJECT:
                            if (IsClient) InternalMessageHandler.HandleSpawnPoolObject(clientId, messageStream, channelId);
                            break;
                        case Constants.ALPACA_DESTROY_POOL_OBJECT:
                            if (IsClient)
                                InternalMessageHandler.HandleDestroyPoolObject(clientId, messageStream, channelId);
                            break;
                        case Constants.ALPACA_CHANGE_OWNER:
                            if (IsClient) InternalMessageHandler.HandleChangeOwner(clientId, messageStream, channelId);
                            break;
                        case Constants.ALPACA_ADD_OBJECTS:
                            if (IsClient) InternalMessageHandler.HandleAddObjects(clientId, messageStream, channelId);
                            break;
                        case Constants.ALPACA_TIME_SYNC:
                            if (IsClient) InternalMessageHandler.HandleTimeSync(clientId, messageStream, channelId);
                            break;
                        case Constants.ALPACA_NETWORKED_VAR_DELTA:
                            InternalMessageHandler.HandleNetworkedVarDelta(clientId, messageStream, channelId);
                            break;
                        case Constants.ALPACA_NETWORKED_VAR_UPDATE:
                            InternalMessageHandler.HandleNetworkedVarUpdate(clientId, messageStream, channelId);
                            break;
                        case Constants.ALPACA_SERVER_RPC:
                            if (IsServer) InternalMessageHandler.HandleServerRPC(clientId, messageStream, channelId);
                            break;
                        case Constants.ALPACA_SERVER_RPC_REQUEST:
                            if (IsServer) InternalMessageHandler.HandleServerRPCRequest(clientId, messageStream, channelId, security);
                            break;
                        case Constants.ALPACA_SERVER_RPC_RESPONSE:
                            if (IsClient) InternalMessageHandler.HandleServerRPCResponse(clientId, messageStream, channelId);
                            break;
                        case Constants.ALPACA_CLIENT_RPC:
                            if (IsClient) InternalMessageHandler.HandleClientRPC(clientId, messageStream, channelId);
                            break;
                        case Constants.ALPACA_CLIENT_RPC_REQUEST:
                            if (IsClient) InternalMessageHandler.HandleClientRPCRequest(clientId, messageStream, channelId, security);
                            break;
                        case Constants.ALPACA_CLIENT_RPC_RESPONSE:
                            if (IsServer) InternalMessageHandler.HandleClientRPCResponse(clientId, messageStream, channelId);
                            break;
                        case Constants.ALPACA_CUSTOM_MESSAGE:
                            InternalMessageHandler.HandleCustomMessage(clientId, messageStream, channelId);
                            break;
#if !DISABLE_CRYPTOGRAPHY
                        case Constants.ALPACA_CERTIFICATE_HAIL:
                            if (IsClient) InternalMessageHandler.HandleHailRequest(clientId, messageStream, channelId);
                            break;
                        case Constants.ALPACA_CERTIFICATE_HAIL_RESPONSE:
                            if (IsServer) InternalMessageHandler.HandleHailResponse(clientId, messageStream, channelId);
                            break;
                        case Constants.ALPACA_GREETINGS:
                            if (IsClient) InternalMessageHandler.HandleGreetings(clientId, messageStream, channelId);
                            break;
#endif
                        default:
                            if (LogHelper.CurrentLogLevel <= LogLevel.Error) LogHelper.LogError("Read unrecognized messageType " + messageType);
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

        internal void OnClientDisconnect( uint clientId )
        {
			if( IsClient )
			{
				_connectedClients.Remove( clientId );
				return;
			}

			bool didRemove = false;
			if( _pendingClients.Get( clientId ) != null)
			{
				didRemove = _pendingClients.Remove( clientId );
			}
			else
			{
				NetworkedClient c = _connectedClients.Get( clientId );
				if( c != null )
				{
                    if( c.PlayerObject != null )
					{
						Destroy( c.PlayerObject.gameObject );
						c.PlayerObject = null;
					}
                    
                    for( int i = 0; i < c.OwnedObjects.Count; ++i )
                    {
						NetworkedObject obj = c.OwnedObjects[i];
						if( obj.DontDestroyWithOwner )
						{
							obj.RemoveOwnership();
						}
						else
						{
							Destroy( obj.gameObject );
						}
                    }
					c.OwnedObjects.Clear();

					didRemove = _connectedClients.Remove( clientId );
                }
            }

            if( didRemove )
            {
                using (PooledBitStream stream = PooledBitStream.Get())
                {
                    using (PooledBitWriter writer = PooledBitWriter.Get(stream))
                    {
                        writer.WriteUInt32Packed(clientId);
                        InternalMessageHandler.Send(Constants.ALPACA_CLIENT_DISCONNECT, "ALPACA_INTERNAL", clientId, stream, SecuritySendFlags.None);
                    }
                }
            }
        }

        private void SyncTime()
        {
            if (LogHelper.CurrentLogLevel <= LogLevel.Developer) LogHelper.LogInfo("Syncing Time To Clients");
            using (PooledBitStream stream = PooledBitStream.Get())
            {
                using (PooledBitWriter writer = PooledBitWriter.Get(stream))
                {
                    writer.WriteSinglePacked(NetworkTime);
                    int timestamp = config.NetworkTransport.GetNetworkTimestamp();
                    writer.WriteInt32Packed(timestamp);
                    InternalMessageHandler.Send(Constants.ALPACA_TIME_SYNC, "ALPACA_TIME_SYNC", stream, SecuritySendFlags.None);
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

			NetworkedClient client = new NetworkedClient()
			{
				ClientId = clientId,
#if !DISABLE_CRYPTOGRAPHY
				AesKey = aesKey
#endif
			};
			_connectedClients.Add(clientId, client);

			int amountOfObjectsToSend = SpawnManager.SpawnedObjects.Values.Count;

			using( PooledBitStream stream = PooledBitStream.Get() )
			{
				using( PooledBitWriter writer = PooledBitWriter.Get(stream) )
				{
					writer.WriteUInt32Packed(clientId);
					writer.WriteSinglePacked(NetworkTime);
					writer.WriteInt32Packed(config.NetworkTransport.GetNetworkTimestamp());
					writer.WriteInt32Packed(amountOfObjectsToSend);

					foreach (KeyValuePair<uint, NetworkedObject> pair in SpawnManager.SpawnedObjects)
					{
						writer.WriteBool(pair.Value.IsPlayerObject);
						writer.WriteUInt32Packed(pair.Value.NetworkId);
						writer.WriteUInt32Packed(pair.Value.OwnerClientId);
						writer.WriteUInt64Packed(pair.Value.NetworkedPrefabHash);
						writer.WriteBool(pair.Value.gameObject.activeInHierarchy);

						writer.WriteSinglePacked(pair.Value.transform.position.x);
						writer.WriteSinglePacked(pair.Value.transform.position.y);
						writer.WriteSinglePacked(pair.Value.transform.position.z);

						writer.WriteSinglePacked(pair.Value.transform.rotation.eulerAngles.x);
						writer.WriteSinglePacked(pair.Value.transform.rotation.eulerAngles.y);
						writer.WriteSinglePacked(pair.Value.transform.rotation.eulerAngles.z);

						pair.Value.WriteNetworkedVarData(stream, clientId);
					}

					InternalMessageHandler.Send(clientId, Constants.ALPACA_CONNECTION_APPROVED, "ALPACA_INTERNAL", stream, SecuritySendFlags.Encrypted | SecuritySendFlags.Authenticated, true);

					if( OnClientConnectedCallback != null ) { OnClientConnectedCallback.Invoke(clientId); }
				}
			}

			// Inform old clients of the new player

			for( int i = 0; i < _connectedClients.GetCount(); ++i )
			{
				NetworkedClient c = _connectedClients.GetAt(i);
				if( c.ClientId == clientId )
				{
					continue; // the new client
				}

				using (PooledBitStream stream = PooledBitStream.Get())
				{
					using (PooledBitWriter writer = PooledBitWriter.Get(stream))
					{
						writer.WriteUInt32Packed(clientId);
						InternalMessageHandler.Send( c.ClientId, Constants.ALPACA_ADD_OBJECT, "ALPACA_INTERNAL", stream, SecuritySendFlags.None );
					}
				}
			}
        }
    }
}
