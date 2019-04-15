using System;

using UnityEngine;


namespace Archer.StateMachine
{
	// State machine class with simple stack of previous states.
	//
	// - POP_STATE_ID and NO_TRANSITION_ID are special values that
	//   must be matched by the sub-classes' state enumeration.
	// - when POP_STATE_ID is returned from GetTransitionID(),
	//   the state machine will return to the state that came
	//   before the current one. It is an error to return this
	//   when there is no previous state.
	// - when NO_TRANSITION_ID is returned from GetTransitionID(),
	//   no state transition will occur. The current state will
	//   be updated again next frame.
	// - Normal states must have IDs from 0 to (capacity - 1)
	//   with no gaps and no numbers re-used.

	public interface FSMState
	{
		void OnEnter();
		void OnExit();
		// used to avoid name clash with MonoBehaviour Update
		void OnUpdate();
		void OnFixedUpdate();

		int GetTransitionID();
		int GetID();
	}

	public class StateMachine<S> where S : class, FSMState
	{
		public const int POP_STATE_ID = -2;
		public const int NO_TRANSITION_ID = -1;

		S[] _state;
		int _currentStateID;
		int[] _stateIDStack;
		int _previousStateCount;


		#region Public

		public StateMachine( int capacity )
		{
			_state = new S[ capacity ];
			_currentStateID = capacity;

			// if we are more than a handful of states deep, we almost certainly have a bug
			_stateIDStack = new int[ 16 ];
			_previousStateCount = 0;
		}

		public void OnUpdate()
		{
			S currentState = GetCurrentState();
			if( currentState != null )
			{
				currentState.OnUpdate();
				TransitionTo( currentState.GetTransitionID() );
			}
			else
			{
				Debug.LogError( "StateMachine: No current state to update. The state machine was not initialized correctly." );
			}
		}

		public void OnFixedUpdate()
		{
			S currentState = GetCurrentState();
			if( currentState != null )
			{
				currentState.OnFixedUpdate();
			}
		}

		public void AddState( S state )
		{
			if( state == null )
			{
				Debug.LogError( "StateMachine: Cannot add null state." );
			}

			int ID = state.GetID();

			if( ID >= _state.Length )
			{
				Debug.LogError( String.Format("StateMachine: State ID out of bounds, failed to add state ID: {0}, capacity: {1}", state.GetID(), _state.Length) ); 
			}

			if( _state[ ID ] != null )
			{
				Debug.LogError( String.Format( "StateMachine: Attempted to add duplicate state ID: {0}", state.GetID() ) );
			}

			_state[ ID ] = state;
		}

		public void TransitionTo( int ID )
		{
			if( ID == NO_TRANSITION_ID )
			{
				// most common case
			}
			else if( ID == POP_STATE_ID )
			{
				PopState();
			}
			else
			{
				TransitionToInternal( ID );
			}
		}

		#endregion

		#region Private

		S GetCurrentState()
		{
			return (_currentStateID < _state.Length) ? _state[ _currentStateID ] : default( S );
		}

		void TransitionToInternal( int ID )
		{
			if( ID < 0 || ID >= _state.Length )
			{
				Debug.LogError( String.Format( "State Machine: TransitionTo state ID: {0} but it is out of range.", ID ) );
			}

			if( ID == _currentStateID )
			{
				// note that we could support transitioning to the same state if there is a reason why the client
				// wants to call OnExit and OnEnter to "reset" their current state.
				Debug.LogWarning( "State Machine: TransitionTo received for same state currently in. Use NO_TRANSITION." );
				return;
			}

			if( _previousStateCount >= _stateIDStack.Length )
			{
				Debug.LogError( "State Machine: Exceeded capacity of state stack, cannot transition." );
			}

			S currentState = GetCurrentState();
			if( currentState != null )
			{
				currentState.OnExit();
				_stateIDStack[ _previousStateCount ] = _currentStateID;
				++_previousStateCount;
			}

			_currentStateID = ID;

			Debug.Log( String.Format( "Transitioned to state: {0} stack length: {1}", ID, _previousStateCount ) );

			currentState = GetCurrentState();
			currentState.OnEnter();
		}

		void PopState()
		{
			if( _previousStateCount <= 0 )
			{
				Debug.LogError( "State Machine: Attempted to pop with empty stack, cannot transition." );
			}

			GetCurrentState().OnExit();

			_currentStateID = _stateIDStack[ _previousStateCount-1 ];
			--_previousStateCount;

			Debug.Log( String.Format( "Popped state to: {0} stack length: {1}", _currentStateID, _previousStateCount ) );

			GetCurrentState().OnEnter();
		}

		#endregion
	}
}