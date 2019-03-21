using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using UnityEngine;

using Alpaca.Data;
using Alpaca.Components;
using Alpaca.Configuration;
using Alpaca.Internal;
using Alpaca.Logging;
using Alpaca.Serialization;


// OLD STUFF

/*
        /// <summary>
        /// Gets if the object is the the personal clients player object
        /// </summary>
        public bool IsLocalPlayer => _networkedObject.IsLocalPlayer;

        /// <summary>
        /// Gets if the object is owned by the local player or if the object is the local player object
        /// </summary>
        public bool IsOwner => _networkedObject.IsOwner;

		/// <summary>
        /// Gets the NetworkId of the Entity that owns the Conduct instance
        /// </summary>
        public uint NetworkId => _networkedObject.NetworkId;

        /// <summary>
        /// Gets the clientId that owns the Entity
        /// </summary>
        public uint OwnerClientId => _networkedObject.OwnerClientId;



        /// <summary>
        /// Gets if we are executing as server
        /// </summary>
        protected bool IsServer => IsRunning && _network.IsServer;

        /// <summary>
        /// Gets if we are executing as client
        /// </summary>
        protected bool IsClient => IsRunning && _network.IsClient;

		private bool IsRunning => _network != null && _network.IsListening;

        /// <summary>
        /// Gets wheter or not the object has a owner
        /// </summary>
        public bool IsOwnedByServer => _networkedObject.IsOwnedByServer;

        /// <summary>
        /// Contains the sender of the currently executing RPC. Useful for the convenience RPC methods
        /// </summary>
        protected uint ExecutingRpcSender { get; private set; }

        /// <summary>
        /// Gets the Entity that owns this Conduct instance
        /// </summary>
        public Entity _networkedObject;


        /// <summary>
        /// Gets behaviourId for this Conduct on this Entity
        /// </summary>
        /// <returns>The behaviourId for the current Conduct</returns>
        public ushort GetBehaviourId()
        {
            return _networkedObject.GetOrderIndex(this);
        }

        /// <summary>
        /// Returns a the Conduct with a given behaviourId for the current networkedObject
        /// </summary>
        /// <param name="id">The behaviourId to return</param>
        /// <returns>Returns Conduct with given behaviourId</returns>
        protected Conduct GetBehaviour(ushort id)
        {
            return _networkedObject.GetBehaviourAtOrderIndex(id);
        }
*/


namespace Alpaca
{
    public abstract partial class Conduct : MonoBehaviour
    {
		// TODO: cozeroff In the future, we would like to compute this once at program start instead of init
		// this is a per-child cache of FieldInfo[] for every SyncVar
		static readonly Dictionary<Type, FieldInfo[]> s_syncVarCache = new Dictionary<Type, FieldInfo[]>();

		protected AlpacaNetwork _network = null;
		protected bool _isLocalOwned;
		protected uint _ownerClientId;
		// our index into the parent entity's array
		// this is needed to do delta updates of networked variables
		protected UInt16 _indexInEntity;  


		List<int> networkedVarIndexesToReset = new List<int>();
        HashSet<int> networkedVarIndexesToResetSet = new HashSet<int>();

		public uint GetOwnerClientId() { return _ownerClientId; }

		public void InitializeNetworkBehaviour( AlpacaNetwork network )
		{
			_network = network;

			// TODO: cozeroff set flags here

			//CacheAttributes();
            SyncVarInitialize();	
			OnInitializeNetworkBehaviour();
		}

		
		// BEGIN Child Interface

		public virtual void OnInitializeNetworkBehaviour() {}                                                                       
        public virtual void OnSyncVarUpdate() {}                                                                                                  

		// END Child Interface


        #region SyncVar

		void SyncVarInitialize()
        {
			// TODO: cozeroff redo this all

			/*

			FieldInfo[] syncVar = ComputeOrGetCachedSyncVarForType( GetType() );
			for( int i = 0; i < syncVar.Length; ++i )
			{
				SyncVar s = (SyncVar)syncVar[i].GetValue(this);
			}



            for( int i = 0; i < field.Length; ++i )
            {
                Type fieldType = sortedFields[i].FieldType;
                if( fieldType.IsSubclassOf( typeof(SyncVarBase) ) )
                {
                    NetVarInterface instance = (NetVarInterface)sortedFields[i].GetValue(this);
                    if (instance == null)
                    {
                        instance = (NetVarInterface)Activator.CreateInstance(fieldType, true);
                        sortedFields[i].SetValue(this, instance);
                    }
                    
                    instance.SetConduct(this);
                    networkedVarFields.Add(instance);
                }
            }

            // Create index map for channels
            Dictionary<string, int> firstLevelIndex = new Dictionary<string, int>();
            int secondLevelCounter = 0;
            for (int i = 0; i < networkedVarFields.Count; i++)
            {
                string channel = networkedVarFields[i].GetChannel(); //Cache this here. Some developers are stupid. You don't know what shit they will do in their methods
                if (!firstLevelIndex.ContainsKey(channel))
                {
                    firstLevelIndex.Add(channel, secondLevelCounter);
                    channelsForVarGroups.Add(channel);
                    secondLevelCounter++;
                }
                if (firstLevelIndex[channel] >= channelMappedVarIndexes.Count)
                    channelMappedVarIndexes.Add(new HashSet<int>());
                channelMappedVarIndexes[firstLevelIndex[channel]].Add(i);
            }
			*/
        }


		/*
        private readonly List<HashSet<int>> channelMappedVarIndexes = new List<HashSet<int>>();
        private readonly List<string> channelsForVarGroups = new List<string>();
        internal readonly List<NetVarInterface> networkedVarFields = new List<NetVarInterface>();
		*/

        
        private static FieldInfo[] GetFieldInfoForType(Type type)
        {
			return null;
			/*
            if (!fieldTypes.ContainsKey(type))
                fieldTypes.Add(type, GetFieldInfoForTypeRecursive(type));
            
            return fieldTypes[type];
			*/
        }
           
        private static FieldInfo[] GetFieldInfoForTypeRecursive(Type type, List<FieldInfo> list = null) 
        {
			/*
            if (list == null) 
            {
                list = new List<FieldInfo>();
                list.AddRange(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
            }
            else
            {
                list.AddRange(type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance));
            }

            if (type.BaseType != null && type.BaseType != typeof(Conduct))
            {
                return GetFieldInfoForTypeRecursive(type.BaseType, list);
            }
            else
            {
                return list.OrderBy(x => x.Name).ToArray();
            }
			*/
			return null;
        }
        
        internal void NetworkedVarUpdate( uint entityId )
        {
			// TODO: cozeroff ARGGHGHGH

			/*
	
            if( _network == null ) { return; }

            //TODO: Do this efficiently.

            if( !CouldHaveDirtyVars() ) { return; }

            networkedVarIndexesToReset.Clear();
            networkedVarIndexesToResetSet.Clear();

			ClientSet client = _network._connectedClients; 
            for (int i = 0; i < client.GetCount(); i++)
            {
                // This iterates over every "channel group".
                for (int j = 0; j < channelMappedVarIndexes.Count; j++)
                {
                    using( PooledBitStream stream = PooledBitStream.Get() )
					using( PooledBitWriter writer = PooledBitWriter.Get(stream) )
					{
						writer.WriteUInt32Packed( entityId );
						writer.WriteUInt16Packed( _indexInEntity );

						uint clientId = client.GetAt(i).GetId();
						bool writtenAny = false;
						for (int k = 0; k < networkedVarFields.Count; k++)
						{
							if (!channelMappedVarIndexes[j].Contains(k))
							{
								//This var does n alse);
								continue;
							}

							bool isDirty = networkedVarFields[k].IsDirty(); //cache this here. You never know what operations users will do in the dirty methods
							writer.WriteBool(isDirty);

							if (isDirty && (!_network.IsServer || networkedVarFields[k].CanClientRead(clientId)))
							{
								writtenAny = true;
								networkedVarFields[k].WriteDelta( writer );
								if (!networkedVarIndexesToResetSet.Contains(k))
								{
									networkedVarIndexesToResetSet.Add(k);
									networkedVarIndexesToReset.Add(k);
								}
							}
						}
						
						if( writtenAny )
						{
							if( _network.IsServer )
							{
								InternalMessageHandler.Send(clientId, AlpacaConstant.ALPACA_NETWORKED_VAR_DELTA, channelsForVarGroups[j], stream, SecuritySendFlags.None);
							}
							else
							{
								InternalMessageHandler.Send(_network.ServerClientId, AlpacaConstant.ALPACA_NETWORKED_VAR_DELTA, channelsForVarGroups[j], stream, SecuritySendFlags.None);   
							}
						}
                    }
                }
            }

            for (int i = 0; i < networkedVarIndexesToReset.Count; i++)
            {
                networkedVarFields[networkedVarIndexesToReset[i]].ResetDirty();
            }

			*/
        }

        private bool CouldHaveDirtyVars()
        {
			return false;
			// TODO: cozeroff arghghghgh
			/*
            for (int i = 0; i < networkedVarFields.Count; i++)
            {
                if (networkedVarFields[i].IsDirty()) 
                    return true;
            }

            return false;
			*/
        }


        internal static void HandleNetworkedVarDeltas(List<SyncVar> networkedVarList, PooledBitReader reader, uint clientId, Conduct logInstance)
        {
			/*

            // TODO: Lot's of performance improvements to do here.
			AlpacaNetwork network = AlpacaNetwork.GetSingleton();

			for( int i = 0; i < networkedVarList.Count; i++ )
			{
				if( !reader.ReadBool() )
				{
					continue;
				}

				if( network.IsServer && !networkedVarList[i].CanClientWrite(clientId) )
				{
					//This client wrote somewhere they are not allowed. This is critical
					//We can't just skip this field. Because we don't actually know how to dummy read
					//That is, we don't know how many bytes to skip. Because the interface doesn't have a 
					//Read that gives us the value. Only a Read that applies the value straight away
					//A dummy read COULD be added to the interface for this situation, but it's just being too nice.
					//This is after all a developer fault. A critical error should be fine.
					// - TwoTen

					if (LogHelper.CurrentLogLevel <= LogLevel.Error) LogHelper.LogError("Client wrote to NetworkedVar without permission. No more variables can be read. This is critical. " + (logInstance != null ? ("NetworkId: " + logInstance.NetworkId + " BehaviourIndex: " + logInstance._networkedObject.GetOrderIndex(logInstance) + " VariableIndex: " + i) : string.Empty));
					return;
				}

				networkedVarList[i].ReadDelta( reader, network.IsServer);
			}
			*/
        }

        internal static void HandleNetworkedVarUpdate(List<SyncVar> networkedVarList, Stream stream, uint clientId, Conduct logInstance)
        {
			/*
			AlpacaNetwork network = AlpacaNetwork.GetSingleton();
            using (PooledBitReader reader = PooledBitReader.Get(stream))
            {
                for (int i = 0; i < networkedVarList.Count; i++)
                {
                    if (!reader.ReadBool())
                        continue;

                    if (network.IsServer && !networkedVarList[i].CanClientWrite(clientId))
                    {
                        //This client wrote somewhere they are not allowed. This is critical
                        //We can't just skip this field. Because we don't actually know how to dummy read
                        //That is, we don't know how many bytes to skip. Because the interface doesn't have a 
                        //Read that gives us the value. Only a Read that applies the value straight away
                        //A dummy read COULD be added to the interface for this situation, but it's just being too nice.
                        //This is after all a developer fault. A critical error should be fine.
                        // - TwoTen
                        if (LogHelper.CurrentLogLevel <= LogLevel.Error) LogHelper.LogError("Client wrote to NetworkedVar without permission. No more variables can be read. This is critical. " + (logInstance != null ? ("NetworkId: " + logInstance.NetworkId + " BehaviourIndex: " + logInstance._networkedObject.GetOrderIndex(logInstance) + " VariableIndex: " + i) : string.Empty));
                        return;
                    }

                    networkedVarList[i].ReadField(reader);
                }
            }
			*/
        }

        internal void WriteNetworkedVarData( PooledBitWriter writer, uint clientId)
        {
			/*
			Debug.Assert( IsInitialized() );

            if (networkedVarFields.Count == 0)
                return;

            for (int j = 0; j < networkedVarFields.Count; j++)
            {
                bool canClientRead = networkedVarFields[j].CanClientRead(clientId);
                writer.WriteBool(canClientRead);
                if (canClientRead) networkedVarFields[j].WriteField( writer );
            }
			*/
        }

        internal void ReadNetworkedVarData( PooledBitReader reader )
        {
			/*
			Debug.Assert( IsInitialized() );

            if (networkedVarFields.Count == 0)
                return;

            for (int j = 0; j < networkedVarFields.Count; j++)
            {
                if (reader.ReadBool()) networkedVarFields[j].ReadField( reader );
            }
			*/
        }


        #endregion

		/*
        #region MESSAGING_SYSTEM
        private readonly Dictionary<Conduct, Dictionary<ulong, ClientRPC>> CachedClientRpcs = new Dictionary<Conduct, Dictionary<ulong, ClientRPC>>();
        private readonly Dictionary<Conduct, Dictionary<ulong, ServerRPC>> CachedServerRpcs = new Dictionary<Conduct, Dictionary<ulong, ServerRPC>>();
        private static readonly Dictionary<Type, MethodInfo[]> Methods = new Dictionary<Type, MethodInfo[]>();
        private static readonly Dictionary<ulong, string> HashResults = new Dictionary<ulong, string>();

        private MethodInfo[] GetNetworkedBehaviorChildClassesMethods(Type type, List<MethodInfo> list = null) 
        {
            if (list == null) 
            {
                list = new List<MethodInfo>();
                list.AddRange(type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
            }
            else
            {
                list.AddRange(type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance));
            }

            if (type.BaseType != null && type.BaseType != typeof(Conduct))
            {
                return GetNetworkedBehaviorChildClassesMethods(type.BaseType, list);
            }
            else
            {
                return list.ToArray();
            }
        }

        private void CacheAttributes()
        {
            Type type = GetType();
            
            CachedClientRpcs.Add(this, new Dictionary<ulong, ClientRPC>());
            CachedServerRpcs.Add(this, new Dictionary<ulong, ServerRPC>());

            MethodInfo[] methods;
            if (Methods.ContainsKey(type)) methods = Methods[type];
            else
            {
                methods = GetNetworkedBehaviorChildClassesMethods(type);
                Methods.Add(type, methods);
            }

            for (int i = 0; i < methods.Length; i++)
            {
                if (methods[i].IsDefined(typeof(ServerRPC), true))
                {
                    ServerRPC[] attributes = (ServerRPC[])methods[i].GetCustomAttributes(typeof(ServerRPC), true);
                    if (attributes.Length > 1)
                    {
                        if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Having more than 1 ServerRPC attribute per method is not supported.");
                    }

                    ParameterInfo[] parameters = methods[i].GetParameters();
                    if (parameters.Length == 2 && parameters[0].ParameterType == typeof(uint) && parameters[1].ParameterType == typeof(Stream) && methods[i].ReturnType == typeof(void))
                    {
                        //use delegate
                        attributes[0].rpcDelegate = (RpcDelegate)Delegate.CreateDelegate(typeof(RpcDelegate), this, methods[i].Name);
                    }
                    else
                    {
                        if (methods[i].ReturnType != typeof(void) && !SerializationHelper.IsTypeSupported(methods[i].ReturnType))
                        {
                            if (LogHelper.CurrentLogLevel <= LogLevel.Error) LogHelper.LogWarning("Invalid return type of RPC. Has to be either void or RpcResponse<T> with a serializable type");
                        }

                        attributes[0].reflectionMethod = new ReflectionMethod(methods[i]);
                    }
                    
                    ulong hash = HashMethodName(methods[i].Name);
                    if (HashResults.ContainsKey(hash) && HashResults[hash] != methods[i].Name)
                    {
                        if (LogHelper.CurrentLogLevel <= LogLevel.Error) LogHelper.LogError($"Hash collision detected for RPC method. The method \"{methods[i].Name}\" collides with the method \"{HashResults[hash]}\". This can be solved by increasing the amount of bytes to use for hashing in the config or changing the name of one of the conflicting methods.");
                    }
                    else if (!HashResults.ContainsKey(hash))
                    {
                        HashResults.Add(hash, methods[i].Name);
                    }

                    CachedServerRpcs[this].Add(hash, attributes[0]);
                }
                
                if (methods[i].IsDefined(typeof(ClientRPC), true))
                {
                    ClientRPC[] attributes = (ClientRPC[])methods[i].GetCustomAttributes(typeof(ClientRPC), true);
                    if (attributes.Length > 1)
                    {
                        if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Having more than 1 ClientRPC attribute per method is not supported.");
                    }

                    ParameterInfo[] parameters = methods[i].GetParameters();
                    if (parameters.Length == 2 && parameters[0].ParameterType == typeof(uint) && parameters[1].ParameterType == typeof(Stream) && methods[i].ReturnType == typeof(void))
                    {
                        //use delegate
                        attributes[0].rpcDelegate = (RpcDelegate)Delegate.CreateDelegate(typeof(RpcDelegate), this, methods[i].Name);
                    }
                    else
                    {
                        if (methods[i].ReturnType != typeof(void) && !SerializationHelper.IsTypeSupported(methods[i].ReturnType))
                        {
                            if (LogHelper.CurrentLogLevel <= LogLevel.Error) LogHelper.LogWarning("Invalid return type of RPC. Has to be either void or RpcResponse<T> with a serializable type");
                        }
                        
                        attributes[0].reflectionMethod = new ReflectionMethod(methods[i]);
                    }

                    ulong hash = HashMethodName(methods[i].Name);
                    if (HashResults.ContainsKey(hash) && HashResults[hash] != methods[i].Name)
                    {
                        if (LogHelper.CurrentLogLevel <= LogLevel.Error) LogHelper.LogError($"Hash collision detected for RPC method. The method \"{methods[i].Name}\" collides with the method \"{HashResults[hash]}\". This can be solved by increasing the amount of bytes to use for hashing in the config or changing the name of one of the conflicting methods.");
                    }
                    else if (!HashResults.ContainsKey(hash))
                    {
                        HashResults.Add(hash, methods[i].Name);
                    }

                    CachedClientRpcs[this].Add(HashMethodName(methods[i].Name), attributes[0]);
                }     
            }
        }

        internal object OnRemoteServerRPC(ulong hash, uint senderClientId, Stream stream)
        {
            if (!CachedServerRpcs.ContainsKey(this) || !CachedServerRpcs[this].ContainsKey(hash))
            {
                if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("ServerRPC request method not found");
                return null;
            }
            
            return InvokeServerRPCLocal(hash, senderClientId, stream);
        }
        
        internal object OnRemoteClientRPC(ulong hash, uint senderClientId, Stream stream)
        {
            if (!CachedClientRpcs.ContainsKey(this) || !CachedClientRpcs[this].ContainsKey(hash))
            {
                if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("ClientRPC request method not found");
                return null;
            }
            
            return InvokeClientRPCLocal(hash, senderClientId, stream);
        }

        private object InvokeServerRPCLocal(ulong hash, uint senderClientId, Stream stream)
        {
            if (!CachedServerRpcs.ContainsKey(this) || !CachedServerRpcs[this].ContainsKey(hash))
                return null;
            
            ServerRPC rpc = CachedServerRpcs[this][hash];

            if (rpc.RequireOwnership && senderClientId != _ownerClientId)
            {
                if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Only owner can invoke ServerRPC that is marked to require ownership");
                return null;
            }

            if (stream.Position != 0)
            {
                // Create a new stream so that the stream they get ONLY contains user data and not headers
                using (PooledBitStream userStream = PooledBitStream.Get())
                {
                    userStream.CopyUnreadFrom(stream);
                    userStream.Position = 0;
                    ExecutingRpcSender = senderClientId;

                    if (rpc.reflectionMethod != null)
                    {
                        ExecutingRpcSender = senderClientId;
                        
                        return rpc.reflectionMethod.Invoke(this, userStream);
                    }

                    if (rpc.rpcDelegate != null)
                    {
                        rpc.rpcDelegate(senderClientId, userStream);
                    }

                    return null;
                }
            }
            else
            {
                ExecutingRpcSender = senderClientId;

                if (rpc.reflectionMethod != null)
                {
                    ExecutingRpcSender = senderClientId;
                    
                    return rpc.reflectionMethod.Invoke(this, stream);
                }

                if (rpc.rpcDelegate != null)
                {
                    rpc.rpcDelegate(senderClientId, stream);
                }

                return null;
            }
        }

        private object InvokeClientRPCLocal(ulong hash, uint senderClientId, Stream stream)
        {
            if (!CachedClientRpcs.ContainsKey(this) || !CachedClientRpcs[this].ContainsKey(hash))
                return null;
            
            ClientRPC rpc = CachedClientRpcs[this][hash];

            if (stream.Position != 0)
            {
                // Create a new stream so that the stream they get ONLY contains user data and not headers
                using (PooledBitStream userStream = PooledBitStream.Get())
                {
                    userStream.CopyUnreadFrom(stream);
                    userStream.Position = 0;

                    if (rpc.reflectionMethod != null)
                    {
                        return rpc.reflectionMethod.Invoke(this, userStream);
                    }

                    if (rpc.rpcDelegate != null)
                    {
                        rpc.rpcDelegate(senderClientId, userStream);
                    }

                    return null;
                }
            }
            else
            {
                if (rpc.reflectionMethod != null)
                {
                    return rpc.reflectionMethod.Invoke(this, stream);
                }

                if (rpc.rpcDelegate != null)
                {
                    rpc.rpcDelegate(senderClientId, stream);
                }

                return null;
            }
        }
        
        //Technically boxed writes are not needed. But save LOC for the non performance sends.
        internal void SendServerRPCBoxed(ulong hash, string channel, SecuritySendFlags security, params object[] parameters)
        {
            using (PooledBitStream stream = PooledBitStream.Get())
            {
                using (PooledBitWriter writer = PooledBitWriter.Get(stream))
                {

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        writer.WriteObjectPacked(parameters[i]);
                    }
                    
                    SendServerRPCPerformance(hash, stream, channel, security);
                }
            }
        }
        
        internal RpcResponse<T> SendServerRPCBoxedResponse<T>(ulong hash, string channel, SecuritySendFlags security, params object[] parameters)
        {
            using (PooledBitStream stream = PooledBitStream.Get())
            {
                using (PooledBitWriter writer = PooledBitWriter.Get(stream))
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        writer.WriteObjectPacked(parameters[i]);
                    }
                    
                    return SendServerRPCPerformanceResponse<T>(hash, stream, channel, security);
                }
            }
        }
        
        internal void SendClientRPCBoxed(ulong hash, uint clientId, string channel, SecuritySendFlags security, params object[] parameters)
        {
            using (PooledBitStream stream = PooledBitStream.Get())
            {
                using (PooledBitWriter writer = PooledBitWriter.Get(stream))
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        writer.WriteObjectPacked(parameters[i]);
                    }
                    SendClientRPCPerformance(hash, clientId, stream, channel, security);
                }
            }
        }
        
        internal RpcResponse<T> SendClientRPCBoxedResponse<T>(ulong hash, uint clientId, string channel, SecuritySendFlags security, params object[] parameters)
        {
            using (PooledBitStream stream = PooledBitStream.Get())
            {
                using (PooledBitWriter writer = PooledBitWriter.Get(stream))
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        writer.WriteObjectPacked(parameters[i]);
                    }
                    
                    return SendClientRPCPerformanceResponse<T>(hash, clientId, stream, channel, security);
                }
            }
        }
        
        internal void SendClientRPCBoxed(ulong hash, string channel, SecuritySendFlags security, params object[] parameters)
        {
            using (PooledBitStream stream = PooledBitStream.Get())
            {
                using (PooledBitWriter writer = PooledBitWriter.Get(stream))
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        writer.WriteObjectPacked(parameters[i]);
                    }
                    SendClientRPCPerformance(hash, stream, channel, security);
                }
            }
        }

        internal void SendClientRPCBoxed(uint clientIdToIgnore, ulong hash, string channel, SecuritySendFlags security, params object[] parameters)
        {
            using (PooledBitStream stream = PooledBitStream.Get())
            {
                using (PooledBitWriter writer = PooledBitWriter.Get(stream))
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        writer.WriteObjectPacked(parameters[i]);
                    }
                    SendClientRPCPerformance(hash, stream, clientIdToIgnore, channel, security);
                }
            }
        }

        internal void SendServerRPCPerformance(ulong hash, Stream messageStream, string channel, SecuritySendFlags security)
        {
            if (!_network.IsClient && _network.IsRunning)
            {
                //We are ONLY a server.
                if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Only server and host can invoke ServerRPC");
                return;
            }

            using (PooledBitStream stream = PooledBitStream.Get())
            {
                using (PooledBitWriter writer = PooledBitWriter.Get(stream))
                {
                    writer.WriteUInt32Packed(NetworkId);
                    writer.WriteUInt16Packed(_networkedObject.GetOrderIndex(this));
                    writer.WriteUInt64Packed(hash);

                    stream.CopyFrom(messageStream);
                    
                    InternalMessageHandler.Send(_network.ServerClientId, AlpacaConstant.ALPACA_SERVER_RPC, string.IsNullOrEmpty(channel) ? "ALPACA_DEFAULT_MESSAGE" : channel, stream, security);
                }
            }
        }
        
        internal RpcResponse<T> SendServerRPCPerformanceResponse<T>(ulong hash, Stream messageStream, string channel, SecuritySendFlags security)
        {
            if (!_network.IsClient && IsRunning)
            {
                //We are ONLY a server.
                if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Only server and host can invoke ServerRPC");
                return null;
            }

            ulong responseId = ResponseMessageManager.GenerateMessageId();

            using (PooledBitStream stream = PooledBitStream.Get())
            {
                using (PooledBitWriter writer = PooledBitWriter.Get(stream))
                {
                    writer.WriteUInt32Packed(NetworkId);
                    writer.WriteUInt16Packed(_networkedObject.GetOrderIndex(this));
                    writer.WriteUInt64Packed(hash);
                    
                    writer.WriteUInt64Packed(responseId);

                    stream.CopyFrom(messageStream);
                        
					RpcResponse<T> response = new RpcResponse<T>()
					{
						Id = responseId,
						IsDone = false,
						IsSuccessful = false,
						Type = typeof(T),
						ClientId = _network.ServerClientId
					};
		
					ResponseMessageManager.Add(response.Id, response);
					
					InternalMessageHandler.Send(_network.ServerClientId, AlpacaConstant.ALPACA_SERVER_RPC_REQUEST, string.IsNullOrEmpty(channel) ? "ALPACA_DEFAULT_MESSAGE" : channel, stream, security);

					return response;
                }
            }
        }

        internal void SendClientRPCPerformance(ulong hash, Stream messageStream, string channel, SecuritySendFlags security)
        {            
            if (!IsServer && IsRunning)
            {
                //We are NOT a server.
                if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Only clients and host can invoke ClientRPC");
                return;
            }

            using (PooledBitStream stream = PooledBitStream.Get())
            {
                using (PooledBitWriter writer = PooledBitWriter.Get(stream))
                {
                    writer.WriteUInt32Packed(NetworkId);
                    writer.WriteUInt16Packed(_networkedObject.GetOrderIndex(this));
                    writer.WriteUInt64Packed(hash);

                    stream.CopyFrom(messageStream);

					for( int i = 0; i < _network._connectedClients.GetCount(); ++i )
					{
						Client c = _network._connectedClients.GetAt(i);
						InternalMessageHandler.Send( c.GetId(), AlpacaConstant.ALPACA_CLIENT_RPC, string.IsNullOrEmpty(channel) ? "ALPACA_DEFAULT_MESSAGE" : channel, stream, security);
					}
                }
            }
        }

        internal void SendClientRPCPerformance(ulong hash, Stream messageStream, uint clientIdToIgnore, string channel, SecuritySendFlags security)
        {
            if (!IsServer && IsRunning)
            {
                //We are NOT a server.
                if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Only clients and host can invoke ClientRPC");
                return;
            }

            using (PooledBitStream stream = PooledBitStream.Get())
            {
                using (PooledBitWriter writer = PooledBitWriter.Get(stream))
                {
                    writer.WriteUInt32Packed(NetworkId);
                    writer.WriteUInt16Packed(_networkedObject.GetOrderIndex(this));
                    writer.WriteUInt64Packed(hash);

                    stream.CopyFrom(messageStream);


                    for (int i = 0; i < _network._connectedClients.GetCount(); i++)
                    {
						Client c = _network._connectedClients.GetAt(i);
                        if( c.GetId() == clientIdToIgnore )
						{
                            continue;
						}                        
                        InternalMessageHandler.Send( c.GetId(), AlpacaConstant.ALPACA_CLIENT_RPC, string.IsNullOrEmpty(channel) ? "ALPACA_DEFAULT_MESSAGE" : channel, stream, security);
                    }
                }
            }
        }

        internal void SendClientRPCPerformance(ulong hash, uint clientId, Stream messageStream, string channel, SecuritySendFlags security)
        {
            if (!IsServer && IsRunning)
            {
                //We are NOT a server.
                if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Only clients and host can invoke ClientRPC");
                return;
            }

            using (PooledBitStream stream = PooledBitStream.Get())
            {
                using (PooledBitWriter writer = PooledBitWriter.Get(stream))
                {
                    writer.WriteUInt32Packed(NetworkId);
                    writer.WriteUInt16Packed(_networkedObject.GetOrderIndex(this));
                    writer.WriteUInt64Packed(hash);

                    stream.CopyFrom(messageStream);

                    InternalMessageHandler.Send(clientId, AlpacaConstant.ALPACA_CLIENT_RPC, string.IsNullOrEmpty(channel) ? "ALPACA_DEFAULT_MESSAGE" : channel, stream, security);
                }
            }
        }
        
        internal RpcResponse<T> SendClientRPCPerformanceResponse<T>(ulong hash, uint clientId, Stream messageStream, string channel, SecuritySendFlags security)
        {
            if (!IsServer && IsRunning)
            {
                //We are NOT a server.
                if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Only clients and host can invoke ClientRPC");
                return null;
            }

            ulong responseId = ResponseMessageManager.GenerateMessageId();
            
            using (PooledBitStream stream = PooledBitStream.Get())
            {
                using (PooledBitWriter writer = PooledBitWriter.Get(stream))
                {
                    writer.WriteUInt32Packed(NetworkId);
                    writer.WriteUInt16Packed(_networkedObject.GetOrderIndex(this));
                    writer.WriteUInt64Packed(hash);             
                    writer.WriteUInt64Packed(responseId);

                    stream.CopyFrom(messageStream);

					RpcResponse<T> response = new RpcResponse<T>()
					{
						Id = responseId,
						IsDone = false,
						IsSuccessful = false,
						Type = typeof(T),
						ClientId = clientId
					};
		
					ResponseMessageManager.Add(response.Id, response);
					
					InternalMessageHandler.Send(clientId, AlpacaConstant.ALPACA_CLIENT_RPC_REQUEST, string.IsNullOrEmpty(channel) ? "ALPACA_DEFAULT_MESSAGE" : channel, stream, security);
					
					return response;
                }
            }
        }
        #endregion
		*/
    }
}
