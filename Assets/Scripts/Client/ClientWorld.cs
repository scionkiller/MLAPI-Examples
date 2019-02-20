using MLAPI;


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


	public ClientWorld( WorldSettings worldSettings, ClientWorldSettings clientWorldSettings )
	{
        _worldSettings = worldSettings;
		_settings = clientWorldSettings;

		_clientRoom = null;
	}

	public NetworkingManager GetNetwork() { return _worldSettings.network; }

	public string GetClientRoom() { return _clientRoom; }
	public void SetClientRoom( string room ) { _clientRoom = room; }
}