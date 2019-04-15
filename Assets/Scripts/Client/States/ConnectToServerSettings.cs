using System.IO;
using System.Text;

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

	char[] _charBuffer;
	StringBuilder _sb;


	#region ClientState interface (including FsmState interface)

	public void Initialize( ClientWorld world, ClientStateSettings settings )
	{
		_world = world;
		_settings = (ConnectToServerSettings)settings;
		_settings.Hide();
		_transitionState = ClientStateId.NO_TRANSITION;
		
		_network = _world.GetClientNode();

		int maxChars = _network.GetMaxMessageLength() / sizeof(char);
		_charBuffer = new char[maxChars];
		_sb = new StringBuilder( maxChars );
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
		CustomMessageType message = (CustomMessageType)reader.Byte();

		if( message != CustomMessageType.ConfirmClientConnection )
		{
			Debug.LogError( "FATAL ERROR: unexpected network message type: " + message );
			return;
		}

		int length = reader.ArrayPacked<char>( _charBuffer );
		_sb.Clear();
		_sb.Append( _charBuffer, 0, length );
		_world.SetClientRoom( _sb.ToString() );
		_settings.display.text += "Server accepted connection. Room scene is: '" + _world.GetClientRoom() + "'\n";

		_connectionSucceeded = true;
		_exitTime = Time.time + MINIMUM_DISPLAY_TIME;
	}
}

} // namespace OneRoom