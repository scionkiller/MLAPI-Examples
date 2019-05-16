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
		_network.SetOnEntitySpawn( OnEntitySpawn );

		_exitTime = float.MaxValue;

		using( BitWriter writer = _network.GetPooledWriter() )
		{
			writer.Normal<byte>( (byte)CustomMessageType.SpawnAvatarRequest );

			string error;
			if( _network.SendCustomServer( writer, true, out error ) )
			{
				_settings.display.text = "Sent spawn request to server, waiting for reply...\n\n";
			}
			else
			{
				_settings.display.text = $"Failed to send spawn request, error:\n{error}\n\n";
			}
		}
	}

	public void OnExit()
	{
		_settings.Hide();
	}

	public void OnUpdate()
	{
		_network.UpdateClient();

		if( Time.time > _exitTime )
		{
			_transitionState = ClientStateId.Playing;
		}
	}

	public void OnFixedUpdate() { }

	public int GetTransitionID() { return (int)_transitionState; }
	public int GetID() { return (int)ClientStateId.SpawnAvatar; }

	#endregion // ClientState interface


	// PRIVATE

	void OnEntitySpawn( Entity entity )
	{
		if(  entity.GetOwner()       != _network.GetLocalNodeIndex()
		  || entity.GetPrefabIndex() != Prefab.PLAYER
		  )
		{
			// ignore any spawns that are not owned by us or that are not players
			Log.Info( $"Server spawned prefab: {entity.GetPrefabIndex().GetIndex()} with owner: {entity.GetOwner().GetDebugString()} but that is not our avatar." );
			return;
		}

		GameObject go = entity.gameObject;
		AvatarSettings avatarSettings = go.GetComponent<AvatarSettings>();
		if( avatarSettings == null )
		{
			_settings.display.text += $"Spawned player entity doesn't have an AvatarSettings behaviour\n\n";
			return;
		}

		Avatar avatar = new Avatar( avatarSettings, _world );
		_settings.display.text += "Successfully spawned player entity with AvatarController\n\n";
		_world.SetAvatar( avatar );
		_exitTime = Time.time + _world.GetMinimumDisplayTime();
	}
}

} // namespace OneRoom