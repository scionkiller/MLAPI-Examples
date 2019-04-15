using System.IO;

using UnityEngine;


// These classes represent network synchronized variables. These allow Conduct to have
// easy-to-use variables they can change in gameplay, that will be automatically synchronized.
//
// To use a SyncVar, simply declare them in your class like this:
//
/*
using Alpaca;
using Alpaca.SyncVar;

class SlideAndSpin : Conduct
{
	[SerializeField]
	SyncVector3 position = null;

	[SerializeField]
	SyncQuaternion rotation = null;

	[SerializeField]
	float _slideSpeedMetersPerSecond = 1f;

	[SerializeField]
	float _rotateSpeedDegreesPerSecond = 90f;

	public override void Initialize()
	{
		// get the list of all the player spawns
		// find one that isn't claimed
		// claim it and move there
	}

	public override void Act( float deltaNetworkTime )
	{
		// slide in a constant direction and spin in a circle
		// note that this is awful code, just used as an example
		position.SetValue( position.GetValue() + Vector3.forward * _slideSpeedMetersPerSecond * deltaNetworkTime );
		Vector3 e = rotation.GetValue().eulerAngles;
		rotation.SetValue( Quaternion.Euler( 0f, e.y + _rotateSpeedDegreesPerSecond * deltaNetworkTime, 0f ) );
	}

	public override void Apply( float deltaNetworkTime )
	{
		transform.position = position.GetValue();
		transform.rotation = rotation.GetValue();
	}
}
*/
//
// Note that SyncVars should always be serialized, so that you can set the SyncVar settings in the
// editor. These include the update rate and the initial value of the SyncVar.
//
// The SyncVar interface is used by the internal networking system to detect propagate and receive
// changes across the network.

namespace Alpaca.Serialization
{

public interface SyncVar
{
	void InitializeSyncVar( uint ownerClientId, float networkTime );
	bool IsDirty( float networkTime );
	string GetChannel();
	bool CanClientWrite( uint clientId );
	bool CanClientRead ( uint clientId );
	void WriteValue( BitWriter writer );
	void ReadValue ( BitReader reader );
	void WriteDelta( BitWriter writer );
	void ReadDelta ( BitReader reader );
}

[System.Serializable]
public abstract class SyncVar<T> : SyncVar where T : struct
{
	public enum Permission
	{
		  Everyone
		, ServerOnly
		, OwnerOnly
	}

	[UnityEngine.SerializeField]
	Permission _writePermission = Permission.ServerOnly;  
	[UnityEngine.SerializeField]
	Permission _readPermission = Permission.Everyone;

	// Less than or equal to 0 will cause the variable to send as soon as possible after being changed.
	[UnityEngine.SerializeField]
	float _maxSendsPerSecond = 60f;

	// The name of the channel to use for this variable.
	// Variables with different channels will be split into different packets
	// TODO: cozeroff this is bullshit, make an array of channel definitions in the config with a name there, and use indices here. Or an enum.
	[UnityEngine.SerializeField]
	string _channel = "INTERNAL_CHANNEL_RELIABLE";

	[UnityEngine.SerializeField]
	T _initialValue = default(T);


	uint _ownerClientId;
	protected  T _value;
	float _lastSyncTimeSeconds;
	// inverse of max sends per second, except zero when _maxSendsPerSecond < epsilon
	float _syncDelaySeconds;
	bool _isDirty;


	public T GetValue() { return _value; }
	public void SetValue( T value )
	{
		if( !_value.Equals( value ) )
		{
			_isDirty = true;
			_value = value;
		}
	}

	// BEGIN SyncVar interface

	public void InitializeSyncVar( uint ownerClientId, float networkTime )
	{
		_ownerClientId = ownerClientId;
		_value = _initialValue;
		_lastSyncTimeSeconds = networkTime;

		if( _maxSendsPerSecond > 0.001f )
		{
			_syncDelaySeconds = 1f / _maxSendsPerSecond;
		}
		else
		{
			_syncDelaySeconds = 0f;
		}
		_isDirty = false;
	}

	public bool IsDirty( float networkTime )
	{
		if( !_isDirty ) { return false; }

		return ((networkTime - _lastSyncTimeSeconds) >= _syncDelaySeconds);
	}

	public string GetChannel() { return _channel; }

	public bool CanClientWrite(uint clientId)
	{
		switch( _writePermission )
		{
			case Permission.Everyone:
				return true;
			case Permission.ServerOnly:
				return false;
			case Permission.OwnerOnly:
				return _ownerClientId == clientId;
			default:
				Debug.Assert(false);
				return false;
		}
	}

	public bool CanClientRead(uint clientId)
	{
		switch( _readPermission )
		{
			case Permission.Everyone:
				return true;
			case Permission.ServerOnly:
				return false;
			case Permission.OwnerOnly:
				return _ownerClientId == clientId;
			default:
				Debug.Assert(false);
				return false;
		}
	}
	
	// read/write all data, must be implemented by child class
	public void WriteValue( BitWriter writer )
	{
		writer.Packed<T>( _value );
	}
	public void ReadValue( BitReader reader )
	{
		_value = reader.Packed<T>();
	}

	// read/write delta since last sync
	// here by default we just write the whole value
	public virtual void WriteDelta( BitWriter writer )
	{
		WriteValue( writer );
	}
	public virtual void ReadDelta( BitReader reader )
	{
		ReadValue( reader );
	}

	// END SyncVar interface
}

// TODO: cozeroff String is not a struct, think this one through
/*public class SyncVarString : SyncVar<string>
{
	public override void ReadData ( PooledBitReader reader ) { _value = reader.ReadString().ToString();  }
	public override void WriteData( PooledBitWriter writer ) { writer.WriteString( _value ); }
}*/

} // namespace Alpaca.SyncVar