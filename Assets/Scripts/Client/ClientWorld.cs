using Alpaca;

using UnityEngine;


[System.Serializable]
public class ClientWorldSettings
{
	public Camera roomCamera = null;
	public AudioListener roomListener = null;
	public ClientNodeSettings clientNodeSettings;
}

// put any data needed to be shared between client states here
// eg. authentication token for the user 
public class ClientWorld
{
	WorldSettings _worldSettings;
	ClientWorldSettings _settings;

	string _clientRoom;
	Avatar _avatar;

	ClientNode _networkNode;
	

	public ClientWorld( WorldSettings worldSettings, ClientWorldSettings clientWorldSettings )
	{
		_worldSettings = worldSettings;
		_settings = clientWorldSettings;

		_networkNode = new ClientNode( _worldSettings.commonNodeSettings, _settings.clientNodeSettings );

		_clientRoom = null;
		_avatar = null;
	}

	public ClientNode GetClientNode() { return _networkNode; }

	public string GetClientRoom() { return _clientRoom; }
	public void SetClientRoom( string room ) { _clientRoom = room; }

	public Avatar GetAvatar() { return _avatar; }
	public void SetAvatar( Avatar avatar ) { _avatar = avatar; }

	// how long to remain in a state after it is completed so that the user can see debug info
	public float GetMinimumDisplayTime() { return _worldSettings.minimumDisplayTime; }

	// we disable the room audio listener after the avatar is spawned
	public void DisableRoomCamera()
	{
		_settings.roomCamera.enabled = false;
		_settings.roomListener.enabled = false;
	}
}
