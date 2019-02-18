using UnityEngine;

using MLAPI;


[System.Serializable]
public class ServerWorldSettings
{
	public NetworkingManager network;
	// TODO: eventually this would be read in from the command line or similar
	public string roomScene;
}

// put any data needed to be shared between client states here
// eg. authentication token for the user 
public class ServerWorld
{
	ServerWorldSettings _settings;


	public ServerWorld( ServerWorldSettings settings )
	{
		_settings = settings;
	}

	public NetworkingManager GetNetwork() { return _settings.network; }
	public string GetRoomName() { return _settings.roomScene; }
}