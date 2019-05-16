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
			public EntityPrefabIndex prefabIndex;
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
				reader.Read( ref data.id          );
				reader.Read( ref data.owner       );
				reader.Read( ref data.prefabIndex );
				position = reader.Packed<Vector3>();
				rotation = reader.Packed<Quaternion>();
			}

			public void Write( BitWriter writer )
			{
				writer.Write( ref data.id          );
				writer.Write( ref data.owner       );
				writer.Write( ref data.prefabIndex );
				writer.Packed<Vector3>( position );
				writer.Packed<Quaternion>( rotation );
			}
		}


		[SerializeField]
		Conduct[] _conduct = null;

		Data _data;                  


		// PUBLIC

		public EntityIndex GetIndex() { return _data.id; }
		public NodeIndex GetOwner() { return _data.owner; }
		public EntityPrefabIndex GetPrefabIndex() { return _data.prefabIndex; }

		public Spawn MakeSpawn() { return new Spawn( _data, transform.position, transform.rotation ); }

		public int GetConductCount() { return _conduct.Length; }
		public Conduct GetConductAt( int conductIndex ) { return _conduct[conductIndex]; }


		// STATIC 

		public static Entity SpawnEntity( Entity[] entityPrefab, Spawn spawn, NodeIndex localNodeIndex )
		{
			Entity prefab = entityPrefab[spawn.data.prefabIndex.GetIndex()];
			Entity entity = GameObject.Instantiate<Entity>(prefab, spawn.position, spawn.rotation);
			Debug.Assert(entity != null);

			entity.InitializeEntity( spawn, localNodeIndex );

			return entity;
		}


		// PRIVATE

		void InitializeEntity( Spawn spawn, NodeIndex localNodeIndex )
		{
			_data = spawn.data;
			Debug.Assert( _data.id.IsValid() );
			Debug.Assert( _data.owner.IsServer() || _data.owner.IsValidClientIndex() );
			Debug.Assert( _data.prefabIndex.IsValid() );

			foreach( Conduct b in _conduct )
			{
				b.InitializeConduct( spawn.data.owner, localNodeIndex );
			}
		}
	}
}