using Alpaca;

using UnityEngine;


[System.Serializable]
public class ClientWorldSettings
{
	public ClientNodeSettings clientNodeSettings;
}

// put any data needed to be shared between client states here
// eg. authentication token for the user 
public class ClientWorld
{
	WorldSettings _worldSettings;
	ClientWorldSettings _settings;

	string _clientRoom;
	AvatarController _avatar;

	ClientNode _networkNode;
	

	public ClientWorld( WorldSettings worldSettings, ClientWorldSettings clientWorldSettings )
	{
		_worldSettings = worldSettings;
		_settings = clientWorldSettings;

		_clientRoom = null;
		_avatar = null;

		_networkNode = null;
	}

	public void StartClientNode() { _networkNode = new ClientNode( _worldSettings.commonNodeSettings, _settings.clientNodeSettings ); }
	public ClientNode GetClientNode() { return _networkNode; }

	public string GetClientRoom() { return _clientRoom; }
	public void SetClientRoom( string room ) { _clientRoom = room; }

	public void SetAvatar( AvatarController avatar ) { _avatar = avatar; }
	public AvatarController GetAvatar() { return _avatar; }
}
