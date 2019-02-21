using UnityEngine;
using UnityEngine.SceneManagement;

using TMPro;
using MLAPI;


public class PlayingSettings : ClientStateSettings
{
	public TMP_Text display;
}

public class Playing : ClientState
{
	ClientWorld _world;
	PlayingSettings _settings;
	ClientStateId _transitionState;

    PlayerInput _playerInput;


	#region ClientState interface (including FsmState interface)

	public void Initialize( ClientWorld world, ClientStateSettings settings )
	{
		_world = world;
		_settings = (PlayingSettings)settings;
		_settings.Hide();
		_transitionState = ClientStateId.NO_TRANSITION;
	}

	public void OnEnter()
	{
		_settings.Show();

        _playerInput = new PlayerInput();

		_settings.display.text = "Ready";
	}

	public void OnExit()
	{
		_settings.Hide();
	}
	
	public void OnUpdate()
	{
        _playerInput.UpdateInput();
        _world.GetAvatar().UpdateAvatar(_playerInput);
	}

	public void OnFixedUpdate() {
        _playerInput.UpdateInput();
        _world.GetAvatar().FixedUpdateAvatar(_playerInput);
    }

	public int GetTransitionID() { return (int)_transitionState; }
	public int GetID() { return (int)ClientStateId.Playing; }

	#endregion // ClientState interface
}