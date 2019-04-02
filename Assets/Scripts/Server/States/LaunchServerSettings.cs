using UnityEngine;
using UnityEngine.SceneManagement;

using TMPro;
using Alpaca;


public class LaunchServerSettings : ServerStateSettings
{
	public TMP_Text display;
}

public class LaunchServer : ServerState
{
	static readonly float MINIMUM_DISPLAY_TIME = 2f;

	ServerWorld _world;
	LaunchServerSettings _settings;
	ServerStateId _transitionState;

	ServerNode _network;

	float _exitTime;


	#region ServerState interface (including FsmState interface)

	public void Initialize( ServerWorld world, ServerStateSettings settings )
	{
		_world = world;
		_network = _world.GetServerNode();

		_settings = (LaunchServerSettings)settings;
		_settings.Hide();

		_transitionState = ServerStateId.NO_TRANSITION;
	}

	public void OnEnter()
	{
		_settings.Show();
		_exitTime = Time.time + MINIMUM_DISPLAY_TIME;

		StartServerAttempt();
	}

	public void OnExit()
	{
		_settings.Hide();
	}
	
	public void OnUpdate()
	{
		if(  _network.IsRunning()
		  && Time.time > _exitTime
		  )
		{
			_transitionState = ServerStateId.Hosting;
		}
	}

	public void OnFixedUpdate() {}

	public int GetTransitionID() { return (int)_transitionState; }
	public int GetID() { return (int)ServerStateId.LaunchServer; }

	#endregion // ServerState interface


	// PRIVATE

	void StartServerAttempt()
	{
		_settings.display.text += "Launching server on port: " + _network.GetConnectionPort();

		string error;
		if( _network.Start( out error ) )
		{
			_settings.display.text += "Server launched successfully.\n";
		}
		else
		{
			_settings.display.text += "Server launch failed with error:\n";
			_settings.display.text += error + "\n\n";
		}
	}
}