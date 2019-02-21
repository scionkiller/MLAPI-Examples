using System.Collections.Generic;
using System.IO;
using MLAPI.Configuration;
using MLAPI.Data;
using MLAPI.Internal;
using MLAPI.Logging;
using MLAPI.NetworkedVar;
using MLAPI.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MLAPI.Components
{
    /// <summary>
    /// Class that handles object spawning
    /// </summary>
    public static class SpawnManager
    {
        public static readonly Dictionary<uint, NetworkedObject> SpawnedObjects = new Dictionary<uint, NetworkedObject>();
        public static readonly List<NetworkedObject> SpawnedObjectsList = new List<NetworkedObject>();

        internal static readonly Stack<uint> releasedNetworkObjectIds = new Stack<uint>();
        private static uint networkObjectIdCounter;
        internal static uint GetNetworkObjectId()
        {
            if (releasedNetworkObjectIds.Count > 0)
            {
                return releasedNetworkObjectIds.Pop();
            }
            else
            {
                networkObjectIdCounter++;
                return networkObjectIdCounter;
            }
        }

        internal static ulong GetPrefabHash(string prefabName)
        {
            HashSize mode = NetworkingManager.GetSingleton().config.PrefabHashSize;

            if (mode == HashSize.VarIntTwoBytes)
                return prefabName.GetStableHash16();
            if (mode == HashSize.VarIntFourBytes)
                return prefabName.GetStableHash32();
            if (mode == HashSize.VarIntEightBytes)
                return prefabName.GetStableHash64();

            return 0;
        }

        /// <summary>
        /// Gets the prefab index of a given prefab hash
        /// </summary>
        /// <param name="hash">The hash of the prefab</param>
        /// <returns>The index of the prefab</returns>
        public static int GetNetworkedPrefabIndexOfHash(ulong hash)
        {
            for (int i = 0; i < NetworkingManager.GetSingleton().config.NetworkedPrefabs.Count; i++)
            {
                if (NetworkingManager.GetSingleton().config.NetworkedPrefabs[i].hash == hash)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Gets the prefab index of a given prefab name
        /// </summary>
        /// <param name="name">The name of the prefab</param>
        /// <returns>The index of the prefab</returns>
        public static int GetNetworkedPrefabIndexOfName(string name)
        {
            for (int i = 0; i < NetworkingManager.GetSingleton().config.NetworkedPrefabs.Count; i++)
            {
                if (NetworkingManager.GetSingleton().config.NetworkedPrefabs[i].name == name)
                    return i;
            }

            return -1;
        }

        private static NetworkingManager netManager => NetworkingManager.GetSingleton();

        /// <summary>
        /// Returns the local player object or null if one does not exist
        /// </summary>
        /// <returns>The local player object or null if one does not exist</returns>
        public static NetworkedObject GetLocalPlayerObject()
        {
            if (!NetworkingManager.GetSingleton().ConnectedClients.ContainsKey(NetworkingManager.GetSingleton().LocalClientId)) return null;
            return NetworkingManager.GetSingleton().ConnectedClients[NetworkingManager.GetSingleton().LocalClientId].PlayerObject;
        }

        /// <summary>
        /// Returns the player object with a given clientId or null if one does not exist
        /// </summary>
        /// <returns>The player object with a given clientId or null if one does not exist</returns>
        public static NetworkedObject GetPlayerObject(uint clientId)
        {
            if (!NetworkingManager.GetSingleton().ConnectedClients.ContainsKey(clientId)) return null;
            return NetworkingManager.GetSingleton().ConnectedClients[clientId].PlayerObject;
        }

        internal static void RemoveOwnership(uint netId)
        {
            if (!netManager.IsServer)
            {
                if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("You can only remove ownership from Server");
                return;
            }
            NetworkedObject netObject = SpawnManager.SpawnedObjects[netId];
            for (int i = NetworkingManager.GetSingleton().ConnectedClients[netObject.OwnerClientId].OwnedObjects.Count - 1; i > -1; i--)
            {
                if (NetworkingManager.GetSingleton().ConnectedClients[netObject.OwnerClientId].OwnedObjects[i].NetworkId == netId)
                    NetworkingManager.GetSingleton().ConnectedClients[netObject.OwnerClientId].OwnedObjects.RemoveAt(i);
            }
			netObject.OwnerClientId = NetworkingManager.GetSingleton().ServerClientId;

            using (PooledBitStream stream = PooledBitStream.Get())
            {
                using (PooledBitWriter writer = PooledBitWriter.Get(stream))
                {
                    writer.WriteUInt32Packed(netId);
                    writer.WriteUInt32Packed(netObject.OwnerClientId);

                    InternalMessageHandler.Send(MLAPIConstants.MLAPI_CHANGE_OWNER, "MLAPI_INTERNAL", stream, SecuritySendFlags.None);
                }
            }
        }

        internal static void ChangeOwnership(uint netId, uint clientId)
        {
            if (!netManager.IsServer)
            {
                if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("You can only change ownership from Server");
                return;
            }
            NetworkedObject netObject = SpawnManager.SpawnedObjects[netId];
            for (int i = NetworkingManager.GetSingleton().ConnectedClients[netObject.OwnerClientId].OwnedObjects.Count - 1; i > -1; i--)
            {
                if (NetworkingManager.GetSingleton().ConnectedClients[netObject.OwnerClientId].OwnedObjects[i].NetworkId == netId)
                    NetworkingManager.GetSingleton().ConnectedClients[netObject.OwnerClientId].OwnedObjects.RemoveAt(i);
            }
            NetworkingManager.GetSingleton().ConnectedClients[clientId].OwnedObjects.Add(netObject);
            netObject.OwnerClientId = clientId;

            using (PooledBitStream stream = PooledBitStream.Get())
            {
                using (PooledBitWriter writer = PooledBitWriter.Get(stream))
                {
                    writer.WriteUInt32Packed(netId);
                    writer.WriteUInt32Packed(clientId);

                    InternalMessageHandler.Send(MLAPIConstants.MLAPI_CHANGE_OWNER, "MLAPI_INTERNAL", stream, SecuritySendFlags.None);
                }
            }
        }

        internal static void DestroyNonSceneObjects()
        {
            if (SpawnedObjects != null)
            {
                foreach (KeyValuePair<uint, NetworkedObject> netObject in SpawnedObjects)
                {
                    MonoBehaviour.Destroy(netObject.Value.gameObject);
                }
            }
        }

        internal static NetworkedObject CreateSpawnedObject( int networkedPrefabId, uint networkId, uint ownerId, bool isPlayerObject, Vector3? position, Quaternion? rotation, bool isActive, Stream stream, bool readPayload, int payloadLength, bool readNetworkedVar )
        {
            if (networkedPrefabId >= netManager.config.NetworkedPrefabs.Count || networkedPrefabId < 0)
            {
                if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Cannot spawn the object, invalid prefabIndex: " + networkedPrefabId);
                return null;
            }

            //Normal spawning
            { 
                GameObject prefab = netManager.config.NetworkedPrefabs[networkedPrefabId].prefab;
                GameObject go = (position == null && rotation == null) ? MonoBehaviour.Instantiate(prefab) : MonoBehaviour.Instantiate(prefab, position.GetValueOrDefault(Vector3.zero), rotation.GetValueOrDefault(Quaternion.identity));

                NetworkedObject netObject = go.GetComponent<NetworkedObject>();
                if( netObject == null )
                {
                    if( LogHelper.CurrentLogLevel <= LogLevel.Normal ) LogHelper.LogWarning("Please add a NetworkedObject component to the root of all spawnable objects");
                    netObject = go.AddComponent<NetworkedObject>();
                }

                if( readNetworkedVar ) { netObject.SetNetworkedVarData(stream); }

                netObject.NetworkedPrefabName = netManager.config.NetworkedPrefabs[networkedPrefabId].name;
                netObject.IsSpawned = true;
                netObject.IsPooledObject = false;

                if( netManager.IsServer )
				{
					netObject.NetworkId = GetNetworkObjectId();
				}
                else
				{
					netObject.NetworkId = networkId;
				}

                netObject.OwnerClientId = ownerId;
                netObject.IsPlayerObject = isPlayerObject;

                SpawnedObjects.Add(netObject.NetworkId, netObject);
                SpawnedObjectsList.Add(netObject);

                if( readPayload )
                {
                    using( PooledBitStream payloadStream = PooledBitStream.Get() )
                    {
                        payloadStream.CopyUnreadFrom(stream, payloadLength);
                        stream.Position += payloadLength;
                        netObject.InvokeBehaviourNetworkSpawn(payloadStream);
                    }
                }
                else
                {
                    netObject.InvokeBehaviourNetworkSpawn(null);
                }
               
                netObject.gameObject.SetActive(isActive);

                return netObject;
            }
        }

        internal static void UnSpawnObject(NetworkedObject netObject)
        {
            if (!netObject.IsSpawned)
            {
                if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Cannot unspawn objects that are not spawned");
                return;
            }
            else if (!netManager.IsServer)
            {
                if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Only server can unspawn objects");
                return;
            }

            OnDestroyObject(netObject.NetworkId, false);
        }

        //Server only
        internal static void SpawnPlayerObject(NetworkedObject netObject, uint clientId, Stream payload = null)
        {
            if (netObject.IsSpawned)
            {
                if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Object already spawned");
                return;
            }
            else if (!netManager.IsServer)
            {
                if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Only server can spawn objects");
                return;
            }
            else if (SpawnManager.GetNetworkedPrefabIndexOfName(netObject.NetworkedPrefabName) == -1)
            {
                if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("The prefab name " + netObject.NetworkedPrefabName + " does not exist as a networkedPrefab");
                return;
            }
            else if (netManager.ConnectedClients[clientId].PlayerObject != null)
            {
                if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Client already have a player object");
                return;
            }
            uint netId = GetNetworkObjectId();
            netObject.NetworkId = netId;
            SpawnedObjects.Add(netId, netObject);
            SpawnedObjectsList.Add(netObject);
            netObject.IsSpawned = true;
            //netObject.sceneSpawnedInIndex = NetworkSceneManager.CurrentActiveSceneIndex;
            netObject.IsPlayerObject = true;
            netManager.ConnectedClients[clientId].PlayerObject = netObject;

            if (payload == null) netObject.InvokeBehaviourNetworkSpawn(null);
            else netObject.InvokeBehaviourNetworkSpawn(payload);

            foreach (var client in netManager.ConnectedClients)
            {
                using (PooledBitStream stream = PooledBitStream.Get())
                {
                    using (PooledBitWriter writer = PooledBitWriter.Get(stream))
                    {
                        writer.WriteBool(true);
                        writer.WriteUInt32Packed(netObject.NetworkId);
                        writer.WriteUInt32Packed(netObject.OwnerClientId);
                        writer.WriteUInt64Packed(netObject.NetworkedPrefabHash);

                        writer.WriteSinglePacked(netObject.transform.position.x);
                        writer.WriteSinglePacked(netObject.transform.position.y);
                        writer.WriteSinglePacked(netObject.transform.position.z);

                        writer.WriteSinglePacked(netObject.transform.rotation.eulerAngles.x);
                        writer.WriteSinglePacked(netObject.transform.rotation.eulerAngles.y);
                        writer.WriteSinglePacked(netObject.transform.rotation.eulerAngles.z);

                        writer.WriteBool(payload != null);
                        if (payload != null)
                        {
                            writer.WriteInt32Packed((int)payload.Length);
                        }

                        netObject.WriteNetworkedVarData(stream, client.Key);

                        if (payload != null) stream.CopyFrom(payload);

                        InternalMessageHandler.Send(client.Key, MLAPIConstants.MLAPI_ADD_OBJECT, "MLAPI_INTERNAL", stream, SecuritySendFlags.None);
                    }
                }
            }
        }


        internal static void SpawnObject( NetworkedObject netObject, uint? clientOwnerId = null, Stream payload = null )
        {
            if (netObject.IsSpawned)
            {
                if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Object already spawned");
                return;
            }
            else if (!netManager.IsServer)
            {
                if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Only server can spawn objects");
                return;
            }
            else if (SpawnManager.GetNetworkedPrefabIndexOfName(netObject.NetworkedPrefabName) == -1)
            {
                if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("The prefab name " + netObject.NetworkedPrefabName + " does not exist as a networkedPrefab");
                return;
            }

            uint netId = GetNetworkObjectId();
            netObject.NetworkId = netId;
            SpawnedObjects.Add(netId, netObject);
            SpawnedObjectsList.Add(netObject);
            netObject.IsSpawned = true;
            //netObject.sceneSpawnedInIndex = NetworkSceneManager.CurrentActiveSceneIndex;

            if (clientOwnerId != null)
            {
                netObject.OwnerClientId = clientOwnerId.Value;
                NetworkingManager.GetSingleton().ConnectedClients[clientOwnerId.Value].OwnedObjects.Add(netObject);
            }

            if (payload == null) netObject.InvokeBehaviourNetworkSpawn(null);
            else netObject.InvokeBehaviourNetworkSpawn(payload);    

            foreach (var client in netManager.ConnectedClients)
            {
                using (PooledBitStream stream = PooledBitStream.Get())
                {
                    using (PooledBitWriter writer = PooledBitWriter.Get(stream))
                    {
                        writer.WriteBool(false);
                        writer.WriteUInt32Packed(netObject.NetworkId);
                        writer.WriteUInt32Packed(netObject.OwnerClientId);
                        writer.WriteUInt64Packed(netObject.NetworkedPrefabHash);

                        writer.WriteSinglePacked(netObject.transform.position.x);
                        writer.WriteSinglePacked(netObject.transform.position.y);
                        writer.WriteSinglePacked(netObject.transform.position.z);

                        writer.WriteSinglePacked(netObject.transform.rotation.eulerAngles.x);
                        writer.WriteSinglePacked(netObject.transform.rotation.eulerAngles.y);
                        writer.WriteSinglePacked(netObject.transform.rotation.eulerAngles.z);

                        writer.WriteBool(payload != null);
                        if (payload != null)
                        {
                            writer.WriteInt32Packed((int)payload.Length);
                        }

                        netObject.WriteNetworkedVarData(stream, client.Key);

                        if (payload != null) stream.CopyFrom(payload);

                        InternalMessageHandler.Send(client.Key, MLAPIConstants.MLAPI_ADD_OBJECT, "MLAPI_INTERNAL", stream, SecuritySendFlags.None);
                    }
                }
            }
        }

        internal static void OnDestroyObject(uint networkId, bool destroyGameObject)
        {
            if( netManager == null ) { return; }

            //Removal of spawned object
            if (!SpawnedObjects.ContainsKey(networkId))
                return;
			if (!SpawnedObjects[networkId].IsOwnedByServer && !SpawnedObjects[networkId].IsPlayerObject && 
			    netManager.ConnectedClients.ContainsKey(SpawnedObjects[networkId].OwnerClientId))
            {
                //Someone owns it.
                for (int i = NetworkingManager.GetSingleton().ConnectedClients[SpawnedObjects[networkId].OwnerClientId].OwnedObjects.Count - 1; i > -1; i--)
                {
                    if (NetworkingManager.GetSingleton().ConnectedClients[SpawnedObjects[networkId].OwnerClientId].OwnedObjects[i].NetworkId == networkId)
                        NetworkingManager.GetSingleton().ConnectedClients[SpawnedObjects[networkId].OwnerClientId].OwnedObjects.RemoveAt(i);
                }
            }
            SpawnedObjects[networkId].IsSpawned = false;

            if (netManager != null && netManager.IsServer)
            {
                releasedNetworkObjectIds.Push(networkId);
                if (SpawnedObjects[networkId] != null)
                {
                    using (PooledBitStream stream = PooledBitStream.Get())
                    {
                        using (PooledBitWriter writer = PooledBitWriter.Get(stream))
                        {
                            writer.WriteUInt32Packed(networkId);

                            InternalMessageHandler.Send(MLAPIConstants.MLAPI_DESTROY_OBJECT, "MLAPI_INTERNAL", stream, SecuritySendFlags.None);
                        }
                    }
                }
            }

            GameObject go = SpawnedObjects[networkId].gameObject;
            if (destroyGameObject && go != null)
                MonoBehaviour.Destroy(go);
            SpawnedObjects.Remove(networkId);
            for (int i = SpawnedObjectsList.Count - 1; i > -1; i--)
            {
                if (SpawnedObjectsList[i].NetworkId == networkId)
                    SpawnedObjectsList.RemoveAt(i);
            }
        }
    }
}