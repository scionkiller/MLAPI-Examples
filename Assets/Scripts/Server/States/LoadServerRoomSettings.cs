using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

using TMPro;
using Alpaca;


public class LoadServerRoomSettings : ServerStateSettings
{
	public TMP_Text display;
}

public class LoadServerRoom : ServerState
{
	static readonly float MINIMUM_DISPLAY_TIME = 2f;

	ServerWorld _world;
	LoadServerRoomSettings _settings;
	ServerStateId _transitionState;

	float _exitTime;
	AsyncOperation _load;


	#region ServerState interface (including FsmState interface)

	public void Initialize( ServerWorld world, ServerStateSettings settings )
	{
		_world = world;
		_settings = (LoadServerRoomSettings)settings;
		_settings.Hide();
		_transitionState = ServerStateId.NO_TRANSITION;
	}

	public void OnEnter()
	{
		_settings.Show();

		_settings.display.text = "Loading room data, please wait...";

		_load = SceneManager.LoadSceneAsync( _world.GetRoomName(), LoadSceneMode.Additive );

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
			_transitionState = ServerStateId.LaunchServer;
		}
	}

	public void OnFixedUpdate() {}

	public int GetTransitionID() { return (int)_transitionState; }
	public int GetID() { return (int)ServerStateId.LoadServerRoom; }

	#endregion // ServerState interface
}
