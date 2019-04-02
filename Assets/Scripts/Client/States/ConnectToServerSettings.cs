using System.IO;

using UnityEngine;

using TMPro;
using Alpaca;
using Alpaca.Serialization;


namespace OneRoom
{

public class ConnectToServerSettings : ClientStateSettings
{
	public TMP_Text display;
}

public class ConnectToServer : ClientState
{
	static readonly float MINIMUM_DISPLAY_TIME = 2f;

	ClientWorld _world;
	ConnectToServerSettings _settings;
	ClientStateId _transitionState;

	ClientNode _network;

	float _exitTime;
	bool _connectionSucceeded;


	#region ClientState interface (including FsmState interface)

	public void Initialize( ClientWorld world, ClientStateSettings settings )
	{
		_world = world;
		_settings = (ConnectToServerSettings)settings;
		_settings.Hide();
		_transitionState = ClientStateId.NO_TRANSITION;
		
		_network = _world.GetClientNode();
	}

	public void OnEnter()
	{
		_settings.Show();
		_settings.display.text = "";
		_connectionSucceeded = false;

        _network.SetOnCustomMessage( OnCustomMessage );

        StartClient();
    }

	public void OnExit()
	{
		_settings.Hide();

		_network.SetOnCustomMessage( null );
	}
	
	public void OnUpdate()
	{
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

	void StartClient()
	{
		_settings.display.text += "Connecting to server at: " + _network.GetConnectionAddress() + ":" + _network.GetConnectionPort() + "\n";

		string error;
		if( _network.Start( out error ) )
		{
			_settings.display.text += "Waiting for reply...\n\n";
		}
		else
		{
			_settings.display.text += "Connection attempt failed with error:\n";
			_settings.display.text += error + "\n\n";
		}
	}

	void OnCustomMessage( NodeIndex clientIndex, BitReader reader )
	{
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

} // namespace OneRoom