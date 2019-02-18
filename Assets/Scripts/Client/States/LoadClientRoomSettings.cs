using UnityEngine;
using UnityEngine.SceneManagement;

using TMPro;
using MLAPI;


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

		_settings.display.text = "Loading room data, please wait...";

		_load = SceneManager.LoadSceneAsync( "Arena", LoadSceneMode.Additive );

		_exitTime = Time.time + MINIMUM_DISPLAY_TIME;
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

		_settings.display.text = "Room load completed.";

		if( Time.time > _exitTime )
		{
			// TODO: next state
		}
	}

	public void OnFixedUpdate() {}

	public int GetTransitionID() { return (int)_transitionState; }
	public int GetID() { return (int)ClientStateId.LoadClientRoom; }

	#endregion // ClientState interface
}