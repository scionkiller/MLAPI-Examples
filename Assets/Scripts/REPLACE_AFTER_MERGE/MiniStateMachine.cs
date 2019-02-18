using UnityEngine;
using System;
using System.Reflection;

// NOTE: Consider later making the matching of functions executed once only for the class, and stored statically.
// Then, the instance based pointers (because they are bound to the member) could be created like this:
// _onEnter[i] = (InitFunc)Delegate.CreateDelegate( typeof(InitFunc), this, s_onEnterInfo[i] );
// 
// Alternatively, we could define InitFunc() like so:
// delegate void InitFunc( T self );
// And the functions could be left un-bound to the instance, instead passing it in.
// I think this alternative is probably the cleanest and fastest, but it would have to be tested.


// The type T must be an enum. Each enumerated name in the
// enum should be used once and only once, and there should
// be no gaps in the enum.
//
// For each Name in the enum, the child class must implement (note, private):
// T OnUpdateName();
// 
// In addition, the client may optionally implement (private again):
// void OnEnterName();
// void OnExitName();
//
// Note that the child is responsible for calling InitializeFSM and UpdateFSM as appropriate.
namespace Archer.StateMachine
{
	public class MiniStateMachine<T> where T : struct
	{
		delegate void InitFunc();
		delegate T UpdateFunc();

		InitFunc[] _onEnter;
		InitFunc[] _onExit;
		UpdateFunc[] _onUpdate;

		int _currentState;

		#region Child Interface

		protected void InitializeFSM( T startState, int stateCount )
		{
			_onEnter = new InitFunc[ stateCount ];
			_onExit = new InitFunc[ stateCount ];
			_onUpdate = new UpdateFunc[ stateCount ];

			string[] enumNames = Enum.GetNames( typeof( T ) );
			MethodInfo[] method = GetType().GetMethods( BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly );

			// This is a dumb N-squared algo, but N should be small, and this should only be called during init
			// See note at top of class for ways to make this more efficient for many instances.
			for( int i = 0; i < stateCount; i++ )
			{
				string e = enumNames[ i ];
				string enterName = "OnEnter" + e;
				string exitName = "OnExit" + e;
				string updateName = "OnUpdate" + e;

				for( int j = 0; j < method.Length; j++ )
				{
					MethodInfo m = method[ j ];
					string name = m.Name;

					if( name == updateName )
					{
						_onUpdate[ i ] = (UpdateFunc)Delegate.CreateDelegate( typeof( UpdateFunc ), this, m );
					}
					else if( name == enterName )
					{
						_onEnter[ i ] = (InitFunc)Delegate.CreateDelegate( typeof( InitFunc ), this, m );
					}
					else if( name == exitName )
					{
						_onExit[ i ] = (InitFunc)Delegate.CreateDelegate( typeof( InitFunc ), this, m );
					}
				}

				// check functions:
				// - updates must exist
				// - inits do not, if they are missing fill in with NullInit()
				if( _onUpdate[ i ] == null )
				{
					Debug.LogError( String.Format("Missing update function '{0}', please make sure it is defined in the child class.", updateName) );
				}

				if( _onEnter[ i ] == null )
				{
					_onEnter[ i ] = NullInit;
				}

				if( _onExit[ i ] == null )
				{
					_onExit[ i ] = NullInit;
				}
			}

			_currentState = Convert.ToInt32( startState );
			_onEnter[ _currentState ]();
		}

		protected void UpdateFSM()
		{
			T next = _onUpdate[ _currentState ]();
			int nextState = Convert.ToInt32( next );

			if( nextState != _currentState )
			{
				_onExit[ _currentState ]();
				_currentState = nextState;
				_onEnter[ _currentState ]();
			}
		}

		#endregion Child Interface

		void NullInit() { }
	}
}