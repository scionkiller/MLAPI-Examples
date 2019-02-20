using MLAPI;


[System.Serializable]
public class ClientWorldSettings
{
	public NetworkingManager network;

	// @siobhan: this might belong in the SpawnPlayerSettings instead
	public Avatar avatarPrefab;
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

	// @siobhan: this should move to whereever we are spawning the player
	public void SpawnAvatar()
	{
		// TODO: generating a random position on a 10 m circle as a proxy for using an actual spawn point
		float randomTau = Random.Range( 0, 2f * Mathf.PI );
		float x = 10f * Mathf.Cos( randomTau );
		float z = 10f * Mathf.Cos( randomTau );

		AvatarController a =  AvatarController.SpawnAvatar( _settings.avatarPrefab, new Vector3( x, 0f, z ) );
		SetAvatar( a );
	}

	public void SetAvatar( AvatarController AvatarController )
	{
		_avatar = AvatarController;
	}
}
