using UnityEngine;

using Archer.StateMachine;


namespace OneRoom
{

public interface ClientState : FSMState
{
	void Initialize( ClientWorld world, ClientStateSettings settings );
}

public enum ClientStateId
{
	  NO_TRANSITION = StateMachine<ClientState>.NO_TRANSITION_ID
	  
	, ConnectToServer
	, LoadClientRoom
	//, JoinRoomCalibrate
	, SpawnAvatar
	, Playing

	, COUNT
}


// This is the entry point of our entire application.
// Nothing else in OneRoom should have an Update function.
// Server and Client should never be used together in the same Unity instance.
public class Client : MonoBehaviour
{
	[SerializeField]
	ClientWorldSettings _clientWorldSettings = null;

	[SerializeField]
	ConnectToServerSettings _connectToServerSettings = null;

	[SerializeField]
	LoadClientRoomSettings _loadRoomSceneSettings = null;

	[SerializeField]
	SpawnAvatarSettings _spawnAvatarSettings = null;

	[SerializeField]
	PlayingSettings _playingSettings = null;


	ClientWorld _world;
	StateMachine<ClientState> _fsm;
	AsyncOperation _load;


	void Start()
	{
		_load = Utility.LoadCommonScene();
	}

	void Update()
	{
		if (_fsm != null)
		{
			_fsm.OnUpdate();
		}
		else
		{
			if( _load.isDone )
			{
				WorldSettings worldSettings = Utility.GetWorldSettingsFromCommonScene();

				// create the world, which is all data that needs to be shared between states
				_world = new ClientWorld(worldSettings, _clientWorldSettings);

				// setup the FSM, creating each state and passing along its associated settings
				_fsm = new StateMachine<ClientState>((int)ClientStateId.COUNT);
				{
					ClientState s;

					s = new ConnectToServer();
					s.Initialize(_world, _connectToServerSettings);
					_fsm.AddState(s);

					s = new LoadClientRoom();
					s.Initialize(_world, _loadRoomSceneSettings);
					_fsm.AddState(s);

					s = new SpawnAvatar();
					s.Initialize(_world, _spawnAvatarSettings);
					_fsm.AddState(s);

					s = new Playing();
					s.Initialize(_world, _playingSettings);
					_fsm.AddState(s);
				}

				// start the FSM
				_fsm.TransitionTo((int)ClientStateId.ConnectToServer);
			}
		}
	}

	void FixedUpdate()
	{
		if (_fsm != null)
		{
			_fsm.OnFixedUpdate();
		}
	}
}

} // namespace OneRoom