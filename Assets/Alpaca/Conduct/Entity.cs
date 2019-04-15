using System;
using System.Collections.Generic;
using System.IO;

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

			public void ReadFrom( BitReader reader )
			{
				reader.Read<EntityIndex      >( ref id          );
				reader.Read<NodeIndex        >( ref owner       );
				reader.Read<EntityPrefabIndex>( ref prefabIndex );
				isAvatar = reader.Packed<bool>();
				position = reader.Packed<Vector3>();
				rotation = reader.Packed<Quaternion>();
			}

			public void WriteTo( BitWriter writer )
			{
				id.Write( writer );
				owner.Write( writer );
				prefabIndex.Write( writer );
				writer.Packed<bool>(isAvatar);
				writer.Packed<Vector3>(position);
				writer.Packed<Quaternion>(rotation);
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