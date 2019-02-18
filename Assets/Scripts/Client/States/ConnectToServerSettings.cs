using UnityEngine;

using TMPro;
using MLAPI;


public class ConnectToServerSettings : ClientStateSettings
{
	public TMP_Text display;
}

public class ConnectToServer : ClientState
{
	static readonly float MINIMUM_DISPLAY_TIME = 2f;
	static readonly int CONNECTION_RETRIES = 3;
	static readonly float CONNECTION_WAIT_TIME = 3f;

	ClientWorld _world;
	ConnectToServerSettings _settings;
	ClientStateId _transitionState;

	NetworkingManager _network;

	float _exitTime;
	float _nextConnectionAttemptTime;
	int _connectionAttempts;
	bool _connectionSucceeded;


	#region ClientState interface (including FsmState interface)

	public void Initialize( ClientWorld world, ClientStateSettings settings )
	{
		_world = world;
		_settings = (ConnectToServerSettings)settings;
		_settings.Hide();
		_transitionState = ClientStateId.NO_TRANSITION;
		
		_network = _world.GetNetwork();
	}

	public void OnEnter()
	{
		_settings.Show();
		_settings.display.text = "";

		_connectionAttempts = 0;
		_connectionSucceeded = false;
		StartConnectionAttempt();

		_exitTime = Time.time + MINIMUM_DISPLAY_TIME;
	}

	public void OnExit()
	{
		_settings.Hide();
	}
	
	public void OnUpdate()
	{
		if(  Time.time > _nextConnectionAttemptTime
		  && _connectionAttempts < CONNECTION_RETRIES
		  )
		{
			StartConnectionAttempt();
		}

		// TODO: listen for handshake here

		if(  _connectionSucceeded
		  && Time.time > _exitTime
		  )
		{
			_transitionState = ClientStateId.LoadClientRoom;
		}
	}

	public void OnFixedUpdate() {}

	public int GetTransitionID() { return (int)_transitionState; }
	public int GetID() { return (int)ClientStateId.ConnectToServer; }

	#endregion // ClientState interface


	// PRIVATE

	void StartConnectionAttempt()
	{
		++_connectionAttempts;
		_nextConnectionAttemptTime = Time.time + CONNECTION_WAIT_TIME;
		_settings.display.text += "Connecting to server at: " + _network.config.ConnectAddress + ":" + _network.config.ConnectPort + " Attempt " + _connectionAttempts + " of " + CONNECTION_RETRIES + "\n";

		string error;
		if( _network.StartClient( out error ) )
		{
			_settings.display.text += "Waiting for reply...\n\n";
		}
		else
		{
			_settings.display.text += "Connection attempt failed with error:\n";
			_settings.display.text += error + "\n\n";
		}
	}
}