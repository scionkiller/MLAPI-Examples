using System;
using System.Collections.Generic;
using System.IO;
using Alpaca.Components;
using Alpaca.Logging;
using Alpaca.Serialization;
using UnityEngine;


namespace Alpaca
{
	/// <summary>
	/// A component used to identify that a GameObject is networked
	/// </summary>
	[AddComponentMenu("Alpaca/NetworkedObject", -99)]
	public sealed class NetworkedObject : MonoBehaviour
	{
		internal static readonly List<NetworkedBehaviour> NetworkedBehaviours = new List<NetworkedBehaviour>();

		// TODO: cozeroff
		// disabled because this doesn't work correctly during a build, and throws false positives
		// perhaps replace with a manually triggered menu item that checks all network prefabs?
		// private void OnValidate()
		// {
		// 	if( string.IsNullOrEmpty(NetworkedPrefabName) )
		// 	{
		// 		if( LogHelper.CurrentLogLevel <= LogLevel.Normal )
		// 		{
		// 			LogHelper.LogWarning("The NetworkedObject " + gameObject.name + " does not have a NetworkedPrefabName. It has been set to the gameObject name");
		// 		}
		// 		NetworkedPrefabName = gameObject.name;
		// 	}
		// }

		/// <summary>
		/// Gets the unique ID of this object that is synced across the network
		/// </summary>
		public uint NetworkId { get; internal set; }
		/// <summary>
		/// Gets the clientId of the owner of this NetworkedObject
		/// </summary>
		public uint OwnerClientId
		{
			get
			{
				if (_ownerClientId == null)
					return NetworkingManager.GetSingleton() != null ? NetworkingManager.GetSingleton().ServerClientId : 0;
				else
					return _ownerClientId.Value;
			}
			internal set
			{
				if (NetworkingManager.GetSingleton() != null && value == NetworkingManager.GetSingleton().ServerClientId)
					_ownerClientId = null;
				else
					_ownerClientId = value;
			}
		}
		private uint? _ownerClientId = null;

		[Tooltip("The prefab name is the name that identifies this prefab. It must be unique and the same across projects if multiple projects are used.")]
		
		public string NetworkedPrefabName = string.Empty;
		/// <summary>
		/// The hash used to identify the NetworkedPrefab, a hash of the NetworkedPrefabName
		/// </summary>
		public ulong NetworkedPrefabHash => SpawnManager.GetPrefabHash(NetworkedPrefabName);


		public bool IsPlayerObject { get; internal set; }

		public bool IsPooledObject { get; internal set; }

		public ushort PoolId { get; internal set; }

		/// <summary>
		/// Gets if the object is the the personal clients player object
		/// </summary>
		public bool IsLocalPlayer => NetworkingManager.GetSingleton() != null && IsPlayerObject && OwnerClientId == NetworkingManager.GetSingleton().LocalClientId;

		/// <summary>
		/// Gets if the object is owned by the local player or if the object is the local player object
		/// </summary>
		public bool IsOwner => NetworkingManager.GetSingleton() != null && OwnerClientId == NetworkingManager.GetSingleton().LocalClientId;

		/// <summary>
		/// Gets wheter or not the object is owned by anyone
		/// </summary>
		public bool IsOwnedByServer => NetworkingManager.GetSingleton() != null && OwnerClientId == NetworkingManager.GetSingleton().ServerClientId;

		/// <summary>
		/// Gets if the object has yet been spawned across the network
		/// </summary>
		public bool IsSpawned { get; internal set; }

		/// <summary>
		/// Wheter or not to destroy this object if it's owner is destroyed.
		/// If false, the objects ownership will be given to the server.
		/// </summary>
		public bool DontDestroyWithOwner;

		private void OnDestroy()
		{
			if (NetworkingManager.GetSingleton() != null)
				SpawnManager.OnDestroyObject(NetworkId, false);
		}

		/// <summary>
		/// Spawns this GameObject across the network. Can only be called from the Server
		/// </summary>
		/// <param name="spawnPayload">The writer containing the spawn payload</param>
		public void Spawn( Stream spawnPayload = null )
		{
			SpawnManager.SpawnObject( this, null, spawnPayload );
		}

		/// <summary>
		/// Unspawns this GameObject and destroys it for other clients. This should be used if the object should be kept on the server
		/// </summary>
		public void UnSpawn()
		{
			SpawnManager.UnSpawnObject(this);
		}

		/// <summary>
		/// Spawns an object across the network with a given owner. Can only be called from server
		/// </summary>
		/// <param name="clientId">The clientId to own the object</param>
		/// <param name="spawnPayload">The writer containing the spawn payload</param>
		public void SpawnWithOwnership( uint clientId, Stream spawnPayload = null )
		{
			SpawnManager.SpawnObject( this, clientId, spawnPayload );
		}

		/// <summary>
		/// Spawns an object across the network and makes it the player object for the given client
		/// </summary>
		/// <param name="clientId">The clientId whos player object this is</param>
		/// <param name="spawnPayload">The writer containing the spawn payload</param>
		public void SpawnAsPlayerObject(uint clientId, Stream spawnPayload = null)
		{
			SpawnManager.SpawnPlayerObject(this, clientId, spawnPayload);
		}

		/// <summary>
		/// Removes all ownership of an object from any client. Can only be called from server
		/// </summary>
		public void RemoveOwnership()
		{
			SpawnManager.RemoveOwnership(NetworkId);
		}
		/// <summary>
		/// Changes the owner of the object. Can only be called from server
		/// </summary>
		/// <param name="newOwnerClientId">The new owner clientId</param>
		public void ChangeOwnership(uint newOwnerClientId)
		{
			SpawnManager.ChangeOwnership(NetworkId, newOwnerClientId);
		}

		internal void InvokeBehaviourOnLostOwnership()
		{
			for (int i = 0; i < childNetworkedBehaviours.Count; i++)
			{
				childNetworkedBehaviours[i].OnLostOwnership();
			}
		}

		internal void InvokeBehaviourOnGainedOwnership()
		{
			for (int i = 0; i < childNetworkedBehaviours.Count; i++)
			{
				childNetworkedBehaviours[i].OnGainedOwnership();
			}
		}

		internal void InvokeBehaviourNetworkSpawn(Stream stream)
		{
			NetworkingManager network = NetworkingManager.GetSingleton();

			for (int i = 0; i < childNetworkedBehaviours.Count; i++)
			{
				//We check if we are it's networkedObject owner incase a networkedObject exists as a child of our networkedObject.
				if( !childNetworkedBehaviours[i].IsInitialized() )
				{
					childNetworkedBehaviours[i].Initialize( network );
				}
			}
		}

		private List<NetworkedBehaviour> _childNetworkedBehaviours;
		internal List<NetworkedBehaviour> childNetworkedBehaviours
		{
			get
			{
				if(_childNetworkedBehaviours == null)
				{
					_childNetworkedBehaviours = new List<NetworkedBehaviour>();
					NetworkedBehaviour[] behaviours = GetComponentsInChildren<NetworkedBehaviour>();
					for (int i = 0; i < behaviours.Length; i++)
					{
						if (behaviours[i]._networkedObject == this)
							_childNetworkedBehaviours.Add(behaviours[i]);
					}
				}
				return _childNetworkedBehaviours;
			}
		}

		internal static void NetworkedVarPrepareSend()
		{
			for (int i = 0; i < NetworkedBehaviours.Count; i++)
			{
				NetworkedBehaviours[i].NetworkedVarUpdate();
			}
		}
		
		internal void WriteNetworkedVarData(Stream stream, uint clientId)
		{
			using (PooledBitWriter writer = PooledBitWriter.Get(stream))
			{
				for (int i = 0; i < childNetworkedBehaviours.Count; i++)
				{
					childNetworkedBehaviours[i].WriteNetworkedVarData( writer, stream, clientId );
				}
			}
		}

		internal void SetNetworkedVarData(Stream stream)
		{
			using (PooledBitReader reader = PooledBitReader.Get(stream))
			{
				for (int i = 0; i < childNetworkedBehaviours.Count; i++)
				{
					childNetworkedBehaviours[i].SetNetworkedVarData( reader, stream );
				}
			}
		}

		internal ushort GetOrderIndex(NetworkedBehaviour instance)
		{
			for (ushort i = 0; i < childNetworkedBehaviours.Count; i++)
			{
				if (childNetworkedBehaviours[i] == instance)
					return i;
			}
			return 0;
		}

		internal NetworkedBehaviour GetBehaviourAtOrderIndex(ushort index)
		{
			if (index >= childNetworkedBehaviours.Count)
			{
				if (LogHelper.CurrentLogLevel <= LogLevel.Error) LogHelper.LogError("Behaviour index was out of bounds. Did you mess up the order of your NetworkedBehaviours?");
				return null;
			}
			return childNetworkedBehaviours[index];
		}
	}
}
