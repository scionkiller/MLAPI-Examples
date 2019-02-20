using UnityEngine;
using UnityEngine.SceneManagement;

using TMPro;
using MLAPI;
using MLAPI.Serialization;
using System.IO;

public class HostingSettings : ServerStateSettings
{
	public TMP_Text display;
    public NetworkedObject avatarPrefab;
}

public class Hosting : ServerState
{
	ServerWorld _world;
	HostingSettings _settings;
	ServerStateId _transitionState;

	NetworkingManager _network;


	#region ServerState interface (including FsmState interface)

	public void Initialize( ServerWorld world, ServerStateSettings settings )
	{
		_world = world;
		_network = _world.GetNetwork();

		_settings = (HostingSettings)settings;
		_settings.Hide();
		_transitionState = ServerStateId.NO_TRANSITION;
	}

	public void OnEnter()
	{
		_settings.Show();

		_settings.display.text = "Ready";

		_network.OnClientConnectedCallback = OnClientConnected;
        _network.OnIncomingCustomMessage += OnCustomMessage;
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

	void OnClientConnected( uint clientId )
	{
		using( PooledBitStream stream = PooledBitStream.Get() )
		{
			BitWriter writer = new BitWriter(stream);
			writer.WriteByte( (byte)MessageType.ConfirmClientConnection );
			writer.WriteString( _world.GetRoomName(), true );

			_network.SendCustomMessage( clientId, stream );
		}

		// TODO: remove
		Debug.Log( "Sent room name: '" + _world.GetRoomName() + "' to client: " + clientId );

	}

    void OnCustomMessage(uint clientId, Stream stream)
    {
        BitReader reader = new BitReader(stream);
        MessageType message = (MessageType)reader.ReadByte();

        if (message != MessageType.SpawnAvatarRequest)
        {
            Debug.LogError("FATAL ERROR: unexpected network message type: " + message);
            return;
        }

        NetworkedObject avatar = NetworkedObject.Instantiate(_settings.avatarPrefab, Vector3.zero, Quaternion.identity);
        avatar.SpawnAsPlayerObject(clientId);
    }

}
