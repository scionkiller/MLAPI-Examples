using System;

using Alpaca.Cryptography;


namespace Alpaca
{
	
	public class ClientSet : ArraySet<uint, Client>
	{
		public ClientSet( int capacity ) : base(capacity)
		{}
	}

	public class PendingClientSet : ArraySet<uint, PendingClient>
	{
		public PendingClientSet( int capacity ) : base(capacity)
		{}
	}

	public class EntitySet : ArraySet< EntityIndex, Entity >
	{
		public EntitySet( int capacity ) : base( capacity )
		{}
	}

	public class PendingClient
    {
		public enum State
        {
        	  PendingHail        // doing the hail handshake
            , PendingConnection  // waiting for the connection
        }

		// same id as later Client
        uint _id;
        EllipticDiffieHellman _keyExchange;
        byte[] _sharedSecretKey; // AES-256
        State _state;

		public PendingClient()
		{
		}

		public uint GetId() { return _id; }

		public State GetState() { return _state; }
		public void SetState( State state ) { _state = state; }
    }

    public class Client
    {
        uint _id;
        Entity _playerAvatar;
        EntitySet _ownedEntity;
        byte[] _sharedSecretKey; // AES-256

		public Client( bool isServer, uint id, Entity playerAvatar, byte[] sharedSecretKey )
		{
			_id = id;
			_playerAvatar = playerAvatar;

			if( isServer )
			{
				_ownedEntity = new EntitySet( AlpacaConstant.CLIENT_OWNER_LIMIT );
				// TODO: add avatar here?!? old code didn't
			}
			else
			{
				_ownedEntity = null;
			}

			_sharedSecretKey = sharedSecretKey;
		}

		public uint GetId() { return _id; }

		public void SetAvatar( Entity playerAvatar ) { _playerAvatar = playerAvatar; }
		public Entity GetAvatar() { return _playerAvatar; }

		public byte[] GetSharedSecretKey() { return _sharedSecretKey; }

		// SERVER ONLY FUNCTIONS (nullref on client)

		public EntitySet GetOwnedEntity() { return _ownedEntity; }

		public void AddOwnedEntity( Entity entity )
		{
			_ownedEntity.Add( entity.GetIndex(), entity );
		}

		public void RemoveOwnedEntity( Entity entity )
		{
			_ownedEntity.Remove( entity.GetIndex() );
		}
    }
}