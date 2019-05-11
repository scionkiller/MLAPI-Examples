using System.IO;
using System.Text;

using UnityEngine;
using UnityEngine.SceneManagement;

using TMPro;

using Alpaca;
using Alpaca.Serialization;


public class HostingSettings : ServerStateSettings
{
	public TMP_Text display;
	// TODO: ensure this is a prefab object only (assets folder not scene)
	public Entity avatarPrefab;
}

public class Hosting : ServerState
{
	ServerWorld _world;
	HostingSettings _settings;
	ServerStateId _transitionState;

	ServerNode _network;

	char[] _charBuffer;
	StringBuilder _sb;


	#region ServerState interface (including FsmState interface)

	public void Initialize( ServerWorld world, ServerStateSettings settings )
	{
		_world = world;
		_network = _world.GetServerNode();

		_settings = (HostingSettings)settings;
		_settings.Hide();
		_transitionState = ServerStateId.NO_TRANSITION;

		int maxChars = _network.GetMaxMessageLength() / sizeof(char);
		_charBuffer = new char[maxChars];
		_sb = new StringBuilder( maxChars );
	}

	public void OnEnter()
	{
		_settings.Show();

		_settings.display.text = "Ready";

		_network.SetOnCustomMessage( OnCustomMessage );
	}

	public void OnExit()
	{
		_settings.Hide();

		_network.SetOnCustomMessage( null );
	}
	
	public void OnUpdate()
	{
		// TODO: other stuff here
		_network.UpdateServer();
	}

	public void OnFixedUpdate() {}

	public int GetTransitionID() { return (int)_transitionState; }
	public int GetID() { return (int)ServerStateId.Hosting; }

	#endregion // ServerState interface


	// PRIVATE

	void OnCustomMessage( NodeIndex clientIndex, BitReader reader )
	{
		CustomMessageType message = (CustomMessageType)reader.Byte();

		switch( message )
		{
			case CustomMessageType.RoomNameRequest:
				SendRoomNameResponse( clientIndex );
				break;
			case CustomMessageType.SpawnAvatarRequest:
				SpawnEntity( clientIndex );
				break;
			default:
				Log.Error( "FATAL ERROR: unexpected network message type: " + message );
			return;
		}
	}

	void SendRoomNameResponse( NodeIndex clientIndex )
	{
		using( BitWriter writer = _network.GetPooledWriter() )
		{
			writer.Normal<byte>( (byte)CustomMessageType.RoomNameResponse );
			
			_sb.Clear();
			_sb.Append( _world.GetRoomName() );
			int length = _sb.Length;
			_sb.CopyTo( 0, _charBuffer, 0, length );
			writer.ArrayPacked<char>( _charBuffer, length );
			
			string error;
			if( !_network.SendCustomClient( clientIndex, writer, false, out error ) )
			{
				Log.Error( $"Failed to send RoomNameResponse to client {clientIndex.GetClientIndex()}, error:\n{error}\n" );
			}
			else
			{
				Log.Info( $"Sent room name: {_world.GetRoomName()} to client: {clientIndex.GetClientIndex()}" );
			}
		}
	}

	void SpawnEntity( NodeIndex clientIndex )
	{
		// TODO: generating a random position on a 10 m circle as a proxy for using an actual spawn point
		float randomTau = Random.Range( 0, 2f * Mathf.PI );
		float x = 10f * Mathf.Cos( randomTau );
		float z = 10f * Mathf.Cos( randomTau );

		EntityPrefabIndex prefabIndex = _network.FindEntityPrefabIndex( _settings.avatarPrefab );
		if( !prefabIndex.IsValid() )
		{
			Debug.LogError( "FATAL ERROR: could not find avatar prefab" );
		}
		// TODO: cozeroff uncomment
		//_network.SpawnEntityServer( clientIndex, prefabIndex, true, new Vector3( x, 0f, z), Quaternion.identity );
	}
}