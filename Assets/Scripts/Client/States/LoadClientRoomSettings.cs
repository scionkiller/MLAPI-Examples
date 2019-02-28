using UnityEngine;
using UnityEngine.SceneManagement;

using TMPro;
using Alpaca;


public class LoadClientRoomSettings : ClientStateSettings
{
	public TMP_Text display;
}

public class LoadClientRoom : ClientState
{
	static readonly float MINIMUM_DISPLAY_TIME = 2f;

	ClientWorld _world;
	LoadClientRoomSettings _settings;
	ClientStateId _transitionState;

    NetworkingManager _network;

    float _exitTime;
	AsyncOperation _load;


	#region ClientState interface (including FsmState interface)

	public void Initialize( ClientWorld world, ClientStateSettings settings )
	{
		_world = world;
		_settings = (LoadClientRoomSettings)settings;
		_settings.Hide();
		_transitionState = ClientStateId.NO_TRANSITION;
	}

	public void OnEnter()
	{
		_settings.Show();

		string room = _world.GetClientRoom();
		if( _world.GetClientRoom() == null )
		{
			Debug.LogError( "FATAL ERROR: Client room not set before reaching LoadClientRoom state" );
		}

		_settings.display.text = "Loading room data, please wait...";

		_load = SceneManager.LoadSceneAsync( room, LoadSceneMode.Additive );

		_exitTime = 0f;
	}

	public void OnExit()
	{
		_load = null;
		_settings.Hide();
	}
	
	public void OnUpdate()
	{
		if( !_load.isDone )
		{
			return;
		}
		else if( _exitTime == 0f )
		{
			_settings.display.text = "Room load completed.";
			_exitTime = Time.time + MINIMUM_DISPLAY_TIME;
		}

		if( Time.time > _exitTime )
		{
			_transitionState = ClientStateId.SpawnAvatar;
		}
	}

	public void OnFixedUpdate() {}

	public int GetTransitionID() { return (int)_transitionState; }
	public int GetID() { return (int)ClientStateId.LoadClientRoom; }

    #endregion // ClientState interface

}