using MLAPI;


[System.Serializable]
public class ServerWorldSettings
{
	// TODO: eventually this would be read in from the command line or similar
	public string roomScene;
}

// put any data needed to be shared between client states here
// eg. authentication token for the user 
public class ServerWorld
{
    WorldSettings _worldSettings;
	ServerWorldSettings _settings;


	public ServerWorld( WorldSettings worldSettings, ServerWorldSettings serverWorldSettings )
	{
        _worldSettings = worldSettings;
		_settings = serverWorldSettings;
	}

	public NetworkingManager GetNetwork() { return _worldSettings.network; }
	public string GetRoomName() { return _settings.roomScene; }
}