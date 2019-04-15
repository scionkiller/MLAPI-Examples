using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.SceneManagement;

using Alpaca.Internal;

using Alpaca.Serialization;

namespace Alpaca.Components
{
	public static class SpawnManager
	{
		public static readonly Dictionary<uint, Entity> SpawnedObjects = new Dictionary<uint, Entity>();
		public static readonly List<Entity> SpawnedObjectsList = new List<Entity>();

		/*
		internal static void ChangeOwnership(uint netId, uint clientId)
		{
			AlpacaNetwork network = AlpacaNetwork.GetSingleton();

			if( !network.IsServer )
			{
				Log.Warn("You can only change ownership from Server");
				return;
			}

			Entity netObject = SpawnManager.SpawnedObjects[netId];
			Client c = network._connectedClients.Get( netObject.GetOwnerClientId() );
			for( int i = 0; i < c.OwnedObjects.Count - 1; ++i )
			{
				if( c.OwnedObjects[i].GetNetworkId() == netId )
				{
					c.OwnedObjects.RemoveAt(i);
					break;
				}
			}

			network._connectedClients[clientId].OwnedObjects.Add(netObject);
			netObject.SetOwnerClientId( clientId );

			using (PooledBitStream stream = PooledBitStream.Get())
			{
				using (PooledBitWriter writer = PooledBitWriter.Get(stream))
				{
					writer.WriteUInt32Packed(netId);
					writer.WriteUInt32Packed(clientId);

					InternalMessageHandler.Send(AlpacaConstant.ALPACA_CHANGE_OWNER, "INTERNAL_CHANNEL_RELIABLE", stream, SecuritySendFlags.None);
				}
			}
		}
		*/

		internal static void DestroyNonSceneObjects()
		{
			if (SpawnedObjects != null)
			{
				foreach (KeyValuePair<uint, Entity> netObject in SpawnedObjects)
				{
					MonoBehaviour.Destroy(netObject.Value.gameObject);
				}
			}
		}

		internal static void OnDestroyObject(uint networkId, bool destroyGameObject)
		{
			/*
			AlpacaNetwork network = AlpacaNetwork.GetSingleton();

			if( network == null ) { return; }

			// Removal of spawned object
			if( !SpawnedObjects.ContainsKey(networkId) )
			{
				return;
			}

			Entity obj = SpawnedObjects[networkId];
			if( !obj.IsOwnedByServer && !obj.IsAvatar() )
			{
				Client c = network._connectedClients.Get( obj.GetOwnerClientId() );
				if( c != null )
				{
					// remove from client owned objects list
					for( int i = 0; i < c.OwnedObjects.Count; ++i )
					{
						if( c.OwnedObjects[i].NetworkId == networkId )
						{
							c.OwnedObjects.RemoveAt(i);
							break;
						}
					}
				}
			}
			obj.IsSpawned = false;

			if (network != null && network.IsServer)
			{
				releasedEntityIds.Push(networkId);
				if (SpawnedObjects[networkId] != null)
				{
					using (PooledBitStream stream = PooledBitStream.Get())
					{
						using (PooledBitWriter writer = PooledBitWriter.Get(stream))
						{
							writer.WriteUInt32Packed(networkId);

							InternalMessageHandler.Send(AlpacaConstant.ALPACA_DESTROY_OBJECT, "INTERNAL_CHANNEL_RELIABLE", stream, SecuritySendFlags.None);
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
			*/
		}
	}
}