using System;
using System.Collections.Generic;
using System.IO;
using Alpaca.Components;
using Alpaca.Serialization;
using UnityEngine;


namespace Alpaca
{
	// All objects shared over the network must have this MonoBehaviour.
	// It must be at the root of the GameObject in question and should have
	// all child Conducts set in the _Conducts list.
	[AddComponentMenu("Alpaca/Entity", -99)]
	public sealed class Entity : MonoBehaviour
	{
		// simple struct used to contain data for spawning and to send it over the network
		// for explanations of members see main class below it
		public struct Spawn
		{
			public uint id;
			public uint ownerClientId;
			public int prefabIndex;
			public bool isAvatar;

			public Vector3 position;
			public Quaternion rotation;


			public Spawn
				(uint idArg
				, uint ownerClientIdArg
				, int prefabIndexArg
				, bool isAvatarArg
				, Vector3 positionArg
				, Quaternion rotationArg
				)
			{
				id = idArg;
				ownerClientId = ownerClientIdArg;
				prefabIndex = prefabIndexArg;
				isAvatar = isAvatarArg;
				position = positionArg;
				rotation = rotationArg;
			}

			public void ReadFrom(PooledBitReader reader)
			{
				id = reader.ReadUInt32Packed();
				ownerClientId = reader.ReadUInt32Packed();
				prefabIndex = reader.ReadInt32Packed();
				isAvatar = reader.ReadBool();

				position.x = reader.ReadSinglePacked();
				position.y = reader.ReadSinglePacked();
				position.z = reader.ReadSinglePacked();

				float xRot = reader.ReadSinglePacked();
				float yRot = reader.ReadSinglePacked();
				float zRot = reader.ReadSinglePacked();
				rotation = Quaternion.Euler(xRot, yRot, zRot);
			}

			public void WriteTo(PooledBitWriter writer)
			{
				writer.WriteUInt32Packed(id);
				writer.WriteUInt32Packed(ownerClientId);
				writer.WriteInt32Packed(prefabIndex);
				writer.WriteBool(isAvatar);

				writer.WriteSinglePacked(position.x);
				writer.WriteSinglePacked(position.y);
				writer.WriteSinglePacked(position.z);

				Vector3 E = rotation.eulerAngles;
				writer.WriteSinglePacked(E.x);
				writer.WriteSinglePacked(E.y);
				writer.WriteSinglePacked(E.z);
			}
		}


		[SerializeField]
		List<Conduct> _Conducts = new List<Conduct>();


		AlpacaNetwork _network;

		uint _id;            // unique across the network
		uint _ownerClientId; // could be a client or the server
		int _prefabIndex;   // index into NetworkConfig.NetworkedPrefabs
		bool _isAvatar;      // Does this object represent the client in the world? The server cannot own player objects.      
		bool _hasAuthority;  // Is this object owned by the client (or server) that is currently executing?


		// STATIC 

		public static Entity SpawnEntity(AlpacaNetwork network, Spawn data)
		{
			Entity prefab = network.config.NetworkedPrefabs[data.prefabIndex];
			Entity entity = GameObject.Instantiate<Entity>(prefab, data.position, data.rotation);
			Debug.Assert(entity != null);

			entity.InitializeEntity(network, data);

			// TODO: cozeroff implement this
			//network.AddEntity(entity.GetId(), entity);

			return entity;
		}


		// PRIVATE

		void InitializeEntity(AlpacaNetwork network, Spawn data)
		{
			_network = network;
			_id = data.id;
			_prefabIndex = data.prefabIndex;
			_isAvatar = data.isAvatar;

			SetOwnerClientId(data.ownerClientId);

			foreach (Conduct b in _Conducts)
			{
				b.InitializeNetworkBehaviour(_network);
			}
		}

		public uint GetId() { return _id; }

		public uint GetOwnerClientId() { return _ownerClientId; }
		public void SetOwnerClientId(uint ownerClientId)
		{
			_ownerClientId = ownerClientId;
			_hasAuthority = (ownerClientId == _network.LocalClientId);

			foreach (Conduct b in _Conducts)
			{
				//b.SetOwnerId( /* TODO */ );
			}
		}

		public bool IsAvatar() { return _isAvatar; }

		public void NetworkedVarUpdate()
		{
			foreach (Conduct b in _Conducts)
			{
				b.NetworkedVarUpdate(_id);
			}
		}

		public void WriteNetworkedVarData(PooledBitWriter writer, uint clientId)
		{
			foreach (Conduct b in _Conducts)
			{
				b.WriteNetworkedVarData(writer, clientId);
			}
		}

		public void ReadNetworkedVarData(PooledBitReader reader)
		{
			foreach (Conduct b in _Conducts)
			{
				b.ReadNetworkedVarData(reader);
			}
		}
	}
}
