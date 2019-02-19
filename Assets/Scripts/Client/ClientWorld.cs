using UnityEngine;

using TMPro;

using MLAPI;


[System.Serializable]
public class ClientWorldSettings
{
	public NetworkingManager network;
}

// put any data needed to be shared between client states here
// eg. authentication token for the user 
public class ClientWorld
{
	ClientWorldSettings _settings;

	string _clientRoom;


	public ClientWorld( ClientWorldSettings clientSettings )
	{
		_settings = clientSettings;

		_clientRoom = null;
	}

	public NetworkingManager GetNetwork() { return _settings.network; }

	public string GetClientRoom() { return _clientRoom; }
	public void SetClientRoom( string room ) { _clientRoom = room; }
}