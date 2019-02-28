using System.IO;

using UnityEngine;

using TMPro;
using Alpaca;
using Alpaca.Serialization;

public class ConnectToServerSettings : ClientStateSettings
{
	public TMP_Text display;
}

public class ConnectToServer : ClientState
{
	static readonly float MINIMUM_DISPLAY_TIME = 2f;
	static readonly int CONNECTION_RETRIES = 1;
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

        _network.OnIncomingCustomMessage += OnCustomMessage;

        _connectionAttempts = 0;
        _connectionSucceeded = false;
        StartConnectionAttempt();
    }

	public void OnExit()
	{
		_settings.Hide();

		_network.OnIncomingCustomMessage -= OnCustomMessage;
	}
	
	public void OnUpdate()
	{
		if( _connectionSucceeded )
		{
			if( Time.time > _exitTime )
			{
				_transitionState = ClientStateId.LoadClientRoom;
			}
		}
		else
		{
			if(  Time.time > _nextConnectionAttemptTime
			  && _connectionAttempts < CONNECTION_RETRIES
			  )
			{
				StartConnectionAttempt();
			}
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

	void OnCustomMessage( uint clientId, Stream stream )
	{
		BitReader reader = new BitReader(stream);
		MessageType message = (MessageType)reader.ReadByte();

		if( message != MessageType.ConfirmClientConnection )
		{
			Debug.LogError( "FATAL ERROR: unexpected network message type: " + message );
			return;
		}

		string room = reader.ReadString(true).ToString();
		_world.SetClientRoom( room );
		_settings.display.text += "Server accepted connection. Room scene is: '" + room + "'\n";

		_connectionSucceeded = true;
		_exitTime = Time.time + MINIMUM_DISPLAY_TIME;
	}
}