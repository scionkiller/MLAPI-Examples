using Alpaca;

using UnityEngine;


[System.Serializable]
public class ClientWorldSettings
{
}

// put any data needed to be shared between client states here
// eg. authentication token for the user 
public class ClientWorld
{
    WorldSettings _worldSettings;
    ClientWorldSettings _settings;

	string _clientRoom;
	AvatarController _avatar;

	public ClientWorld( WorldSettings worldSettings, ClientWorldSettings clientWorldSettings )
	{
        _worldSettings = worldSettings;
		_settings = clientWorldSettings;

		_clientRoom = null;
	}

	public NetworkingManager GetNetwork() { return _worldSettings.network; }

	public string GetClientRoom() { return _clientRoom; }
	public void SetClientRoom( string room ) { _clientRoom = room; }

	public void SetAvatar( AvatarController avatar ) { _avatar = avatar; }
	public AvatarController GetAvatar() { return _avatar; }
}
