using Alpaca;


[System.Serializable]
public class ServerWorldSettings
{
	public ServerNodeSettings serverNodeSettings;
	// TODO: eventually this would be read in from the command line or similar
	public string roomScene;
}

// put any data needed to be shared between client states here
// eg. authentication token for the user 
public class ServerWorld
{
    WorldSettings _worldSettings;
	ServerWorldSettings _settings;

	ServerNode _networkNode;


	public ServerWorld( WorldSettings worldSettings, ServerWorldSettings serverWorldSettings )
	{
        _worldSettings = worldSettings;
		_settings = serverWorldSettings;
		_networkNode = new ServerNode( _worldSettings.commonNodeSettings, _settings.serverNodeSettings );
	}

	public ServerNode GetServerNode() { return _networkNode; }
	public string GetRoomName() { return _settings.roomScene; }
}