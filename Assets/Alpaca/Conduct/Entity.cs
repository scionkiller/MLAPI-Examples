using System;
using System.Collections.Generic;
using System.IO;

using Alpaca.Serialization;
using UnityEngine;


namespace Alpaca
{
	// All objects shared over the network must have this MonoBehaviour.
	// It must be at the root of the GameObject in question and should have
	// all child Conducts set in the _conduct list.
	[AddComponentMenu("Alpaca/Entity")]
	public sealed class Entity : MonoBehaviour
	{
		public struct Data
		{
			// unique across the network
			public EntityIndex id;
			public NodeIndex owner;
			public EntityPrefabIndex prefabId;
		}

		// Contains all data for spawning entities and sending them over the network
		public struct Spawn : IBitSerializable
		{
			public Data data;
			public Vector3 position;
			public Quaternion rotation;

			public Spawn( Data dataArg, Vector3 positionArg, Quaternion rotationArg )
			{
				data = dataArg;
				position = positionArg;
				rotation = rotationArg;
			}

			public void Read( BitReader reader )
			{
				reader.Read <EntityIndex      >( ref data.id       );
				reader.Read <NodeIndex        >( ref data.owner    );
				reader.Read <EntityPrefabIndex>( ref data.prefabId );
				position = reader.Packed<Vector3>();
				rotation = reader.Packed<Quaternion>();
			}

			public void Write( BitWriter writer )
			{
				writer.Write( ref data.id );
				writer.Write<EntityIndex      >( ref data.id       );
				writer.Write<NodeIndex        >( ref data.owner    );
				writer.Write<EntityPrefabIndex>( ref data.prefabId );
				writer.Packed<Vector3>( position );
				writer.Packed<Quaternion>( rotation );
			}
		}


		[SerializeField]
		Conduct[] _conduct = null;

		Data _data;                  


		// PUBLIC

		public int GetConductCount() { return _conduct.Length; }
		public Conduct GetConductAt( int conductIndex ) { return _conduct[conductIndex]; }

		public EntityIndex GetIndex() { return _data.id; }

		public Spawn MakeSpawn() { return new Spawn( _data, transform.position, transform.rotation ); }


		// STATIC 

		public static Entity SpawnEntity( List<Entity> entityPrefab, Spawn spawn, NodeIndex localNodeIndex )
		{
			Entity prefab = entityPrefab[spawn.data.prefabId.GetIndex()];
			Entity entity = GameObject.Instantiate<Entity>(prefab, spawn.position, spawn.rotation);
			Debug.Assert(entity != null);

			entity.InitializeEntity( spawn, localNodeIndex );

			return entity;
		}


		// PRIVATE

		void InitializeEntity( Spawn spawn, NodeIndex localNodeIndex )
		{
			_data = spawn.data;

			foreach( Conduct b in _conduct )
			{
				b.InitializeConduct( spawn.data.owner, localNodeIndex );
			}
		}
	}
}