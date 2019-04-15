using UnityEngine;
using UnityEngine.SceneManagement;

using TMPro;
using Alpaca;
using Alpaca.Serialization;


namespace OneRoom
{

public class SpawnAvatarSettings : ClientStateSettings
{
	public TMP_Text display;
}

public class SpawnAvatar : ClientState
{
	static readonly float MINIMUM_DISPLAY_TIME = 2f;

	ClientWorld _world;
	SpawnAvatarSettings _settings;
	ClientStateId _transitionState;

	ClientNode _network;
	float _exitTime;


	#region ClientState interface (including FsmState interface)

	public void Initialize(ClientWorld world, ClientStateSettings settings)
	{
		_world = world;
		_network = _world.GetClientNode();
		_settings = (SpawnAvatarSettings)settings;
		_settings.Hide();
		_transitionState = ClientStateId.NO_TRANSITION;
	}

	public void OnEnter()
	{
		_settings.Show();
		_settings.display.text = "Sent spawn request to server...\n";

		_exitTime = 0f;

		_network.SetOnAvatarSpawn( OnAvatarSpawn );

		using( BitWriter writer = _network.GetPooledWriter() )
		{
			writer.Normal<byte>( (byte)CustomMessageType.SpawnAvatarRequest );

			// TODO: cozeroff
			//_network.SendCustomMessage( NodeIndex.SERVER_NODE_INDEX, stream);
		}
	}

	public void OnExit()
	{
		_network.SetOnAvatarSpawn( null );

		_settings.Hide();
	}

	public void OnUpdate()
	{
		if( _world.GetAvatar() != null && Time.time > _exitTime )
		{
			_transitionState = ClientStateId.Playing;
		}
	}

	public void OnFixedUpdate() { }

	public int GetTransitionID() { return (int)_transitionState; }
	public int GetID() { return (int)ClientStateId.SpawnAvatar; }

	#endregion // ClientState interface


	// PRIVATE

	void OnAvatarSpawn( Entity spawnedPrefab )
	{
		Avatar a = spawnedPrefab.GetComponent<Avatar>();
		if( a == null )
		{
			_settings.display.text += "FATAL ERROR: Spawned prefab does not have an Avatar component!\n";
			return;
		}

		AvatarController c = new AvatarController( a );
		_world.SetAvatar( c );
		_settings.display.text += "Spawned avatar successfully.\n";
		_exitTime = Time.time + MINIMUM_DISPLAY_TIME;
	}
}

} // namespace OneRoom