using UnityEngine;

using Archer.StateMachine;


public interface ServerState : FSMState
{
	void Initialize( ServerWorld world, ServerStateSettings settings );
}

public enum ServerStateId
{
	  NO_TRANSITION = StateMachine<ServerState>.NO_TRANSITION_ID
	  
	, LoadServerRoom
	, LaunchServer
	, Hosting

	, COUNT
}


// This is the entry point of our entire application.
// Nothing else in OneRoom should have an Update function.
// Server and Client should never be used together in the same Unity instance.
public class Server : MonoBehaviour
{
	[SerializeField]
	ServerWorldSettings _worldSettings = null;

	[SerializeField]
	LoadServerRoomSettings _loadServerRoomSettings = null;

	[SerializeField]
	LaunchServerSettings _launchServerSettings = null;

	[SerializeField]
	HostingSettings _hostingSettings = null;


	ServerWorld _world;
	StateMachine<ServerState> _fsm;


    void Start()
    {
		// create the world, which is all data that needs to be shared between states
		_world = new ServerWorld( _worldSettings );

		// setup the FSM, creating each state and passing along its associated settings
        _fsm = new StateMachine<ServerState>( (int)ServerStateId.COUNT );
		{
			ServerState s;

			s = new LoadServerRoom();
			s.Initialize( _world, _loadServerRoomSettings );
			_fsm.AddState(s);

			s = new LaunchServer();
			s.Initialize( _world, _launchServerSettings );
			_fsm.AddState(s);

			s = new Hosting();
			s.Initialize( _world, _hostingSettings );
			_fsm.AddState(s);
		}

		// start the FSM
		_fsm.TransitionTo( (int)ServerStateId.LoadServerRoom );
    }

    void Update()
    {
		_fsm.OnUpdate();
    }

	void FixedUpdate()
	{
		_fsm.OnFixedUpdate();
	}
}