using System.IO;

using UnityEngine;
using UnityEngine.SceneManagement;

using TMPro;

using Alpaca;
using Alpaca.Serialization;

public class HostingSettings : ServerStateSettings
{
	public TMP_Text display;
	// TODO: ensure this is a prefab object only (assets folder not scene)
    public Entity avatarPrefab;
}

public class Hosting : ServerState
{
	ServerWorld _world;
	HostingSettings _settings;
	ServerStateId _transitionState;

	ServerNode _network;


	#region ServerState interface (including FsmState interface)

	public void Initialize( ServerWorld world, ServerStateSettings settings )
	{
		_world = world;
		_network = _world.GetServerNode();

		_settings = (HostingSettings)settings;
		_settings.Hide();
		_transitionState = ServerStateId.NO_TRANSITION;
	}

	public void OnEnter()
	{
		_settings.Show();

		_settings.display.text = "Ready";

		_network.SetOnClientConnect( OnClientConnected );
		_network.SetOnCustomMessage( OnCustomMessage );
    }

	public void OnExit()
	{
		_settings.Hide();

		_network.OnClientConnectedCallback = null;
        _network.OnIncomingCustomMessage -= OnCustomMessage;
    }
	
	public void OnUpdate()
	{
	}

	public void OnFixedUpdate() {}

	public int GetTransitionID() { return (int)_transitionState; }
	public int GetID() { return (int)ServerStateId.Hosting; }

	#endregion // ServerState interface


	// PRIVATE

	void OnClientConnected( NodeId clientId )
	{
		using( PooledBitStream stream = PooledBitStream.Get() )
		using( PooledBitWriter writer = PooledBitWriter.Get( stream ) )
		{
			writer.WriteByte( (byte)MessageType.ConfirmClientConnection );
			writer.WriteString( _world.GetRoomName(), true );

			_network.SendCustomMessage( clientId, writer );
		}

		// TODO: remove
		Debug.Log( "Sent room name: '" + _world.GetRoomName() + "' to client: " + clientId );

	}

    void OnCustomMessage( uint receiverId, BitReader reader )
    {
        MessageType message = (MessageType)reader.ReadByte();
		uint clientId = reader.ReadUInt32();

        if( message != MessageType.SpawnAvatarRequest )
        {
            Debug.LogError( "FATAL ERROR: unexpected network message type: " + message );
            return;
        }

		// TODO: generating a random position on a 10 m circle as a proxy for using an actual spawn point
		float randomTau = Random.Range( 0, 2f * Mathf.PI );
		float x = 10f * Mathf.Cos( randomTau );
		float z = 10f * Mathf.Cos( randomTau );

		int prefabIndex = _network.FindPrefabIndex( _settings.avatarPrefab );
		if( prefabIndex == AlpacaConstant.PREFAB_INDEX_INVALID )
		{
			Debug.LogError( "FATAL ERROR: could not found avatar prefab" );
		}
		_network.SpawnEntityServer( clientId, prefabIndex, true, new Vector3( x, 0f, z), Quaternion.identity );
    }

}
