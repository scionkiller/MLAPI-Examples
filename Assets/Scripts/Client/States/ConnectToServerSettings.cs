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
	bool _sentRoomNameRequest;
	bool _roomNameSet;

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
		_sentRoomNameRequest = false;
		_roomNameSet = false;

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
		_network.UpdateClient();

		if( _network.IsConnectedToServer() && !_sentRoomNameRequest )
		{
			SendRoomNameRequest();
		}

		if(  _roomNameSet 
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
		_settings.display.text += "Connecting to server at: " + _network.GetConnectionAddress() + ":" + _network.GetServerPort() + "\n";

		string error;
		if( _network.Start( out error ) )
		{
			_settings.display.text += "Waiting for reply...\n\n";
		}
		else
		{
			_settings.display.text += $"Connection attempt failed with error:\n{error}\n\n";
		}
	}

	void SendRoomNameRequest()
	{
		_settings.display.text += "Successfully connected to server, sending request for room name.\n";
		using( BitWriter writer = _network.GetPooledWriter() )
		{
			writer.Normal<byte>( (byte)CustomMessageType.RoomNameRequest );
			string error;
			if( !_network.SendCustomServer( writer, false, out error ) )
			{
				_settings.display.text += $"Failed to send room request: \n{error}\n\n";
			}
			else
			{
				_sentRoomNameRequest = true;
			}
		}
	}

	void OnCustomMessage( NodeIndex clientIndex, BitReader reader )
	{
		CustomMessageType message = (CustomMessageType)reader.Byte();

		if( message != CustomMessageType.RoomNameResponse )
		{
			_settings.display.text += $"Unexpected custom message type: {message}\n";
			return;
		}

		int length = reader.ArrayPacked<char>( _charBuffer );
		_sb.Clear();
		_sb.Append( _charBuffer, 0, length );
		_world.SetClientRoom( _sb.ToString() );
		_settings.display.text += "Got room scene from server: '" + _world.GetClientRoom() + "'\n";

		_roomNameSet = true;
		_exitTime = Time.time + MINIMUM_DISPLAY_TIME;
	}
}

} // namespace OneRoom