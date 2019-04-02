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
	[AddComponentMenu("Alpaca/Entity")]
	public sealed class Entity : MonoBehaviour
	{
		// simple struct used to contain data for spawning and to send it over the network
		// for explanations of members see main class below it
		public struct Spawn
		{
			public EntityIndex id;
			public NodeIndex owner;
			public EntityPrefabIndex prefabIndex;
			public bool isAvatar;

			public Vector3 position;
			public Quaternion rotation;


			public Spawn
				( EntityIndex idArg
				, NodeIndex ownerArg
				, EntityPrefabIndex prefabIndexArg
				, bool isAvatarArg
				, Vector3 positionArg
				, Quaternion rotationArg
				)
			{
				id = idArg;
				owner = ownerArg;
				prefabIndex = prefabIndexArg;
				isAvatar = isAvatarArg;
				position = positionArg;
				rotation = rotationArg;
			}

			public void ReadFrom(PooledBitReader reader)
			{
				id.ReadFrom( reader );
				owner.ReadFrom( reader );
				prefabIndex.ReadFrom( reader );
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
				id.WriteTo( writer );
				owner.WriteTo( writer );
				prefabIndex.WriteTo( writer );
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
		Conduct[] _conduct = null;

		EntityIndex _id;                 // unique across the network
		EntityPrefabIndex _prefabIndex; 
		bool _isAvatar;                  // Does this object represent the client in the world? The server cannot own player objects.      


		// STATIC 

		public static Entity SpawnEntity( List<Entity> entityPrefab, Spawn data, NodeIndex localNodeIndex )
		{
			Entity prefab = entityPrefab[data.prefabIndex.GetIndex()];
			Entity entity = GameObject.Instantiate<Entity>(prefab, data.position, data.rotation);
			Debug.Assert(entity != null);

			entity.InitializeEntity( data, localNodeIndex );

			return entity;
		}


		// PRIVATE

		void InitializeEntity( Spawn data, NodeIndex localNodeIndex )
		{
			_id = data.id;
			_prefabIndex = data.prefabIndex;
			_isAvatar = data.isAvatar;

			foreach( Conduct b in _conduct )
			{
				b.InitializeConduct( data.owner, localNodeIndex );
			}
		}

		int GetConductCount() { return _conduct.Length; }
		Conduct GetConductAt( int conductIndex ) { return _conduct[conductIndex]; }

		public EntityIndex GetIndex() { return _id; }
		public bool IsAvatar() { return _isAvatar; }
	}
}