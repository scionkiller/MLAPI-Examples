using UnityEngine;
using UnityEngine.SceneManagement;

using TMPro;
using MLAPI;


public class HostingSettings : ServerStateSettings
{
	public TMP_Text display;
}

public class Hosting : ServerState
{
	ServerWorld _world;
	HostingSettings _settings;
	ServerStateId _transitionState;


	#region ServerState interface (including FsmState interface)

	public void Initialize( ServerWorld world, ServerStateSettings settings )
	{
		_world = world;
		_settings = (HostingSettings)settings;
		_settings.Hide();
		_transitionState = ServerStateId.NO_TRANSITION;
	}

	public void OnEnter()
	{
		_settings.Show();

		_settings.display.text = "Ready";
	}

	public void OnExit()
	{
		_settings.Hide();
	}
	
	public void OnUpdate()
	{
		
	}

	public void OnFixedUpdate() {}

	public int GetTransitionID() { return (int)_transitionState; }
	public int GetID() { return (int)ServerStateId.Hosting; }

	#endregion // ServerState interface
}
