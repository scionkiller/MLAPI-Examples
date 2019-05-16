/*
using UnityEngine;

using Alpaca;
using Alpaca.Serialization;



public interface IConduct
{
	void Initialize();

	void Predict();
	void Act();

	void Send( BitWriter writer );
	void Receive( BitReader reader );

	void Apply();

	// Ownership?
	bool AmITheOwner();
}




class NetTransform : MonoBehaviour, IConduct
{
	[SerializeField]
	SyncVar<Vector3> _position = null;

	[SerializeField]
	SyncVar<Quaternion> _rotation = null;


	public void Initialize()
	{
		_position.SetValue( Vector3.zero );
		_rotation.SetValue( Quaternion.identity );
	}

	// THIS IS LOCAL ONLY - Every Frame
	public void Act( float deltaNetworkTime )
	{

	}

	// THIS IS REMOTE ONLY - Every Frame
	public void Predict()
	{

	}

	// THIS IS LOCAL ONLY - only when needed
	void Send( BitWriter writer )
	{
		_position.WriteDelta( writer );
		_rotation.WriteDelta( writer );
	}

	// THIS IS REMOTE ONLY - only when needed
	void Receive( BitReader reader )
	{
		_position.ReadDelta( reader );
		_rotation.ReadDelta( reader );
	}

	// BOTH OF THEM - Every Frame
	public void Apply( float deltaNetworkTime )
	{
		transform.position = _position.GetValue();
		transform.rotation = _rotation.GetValue();
	}
}





void PlayerBehaviour
{
	[SerializeField]
	NetTransform _head;

	[SerializeField]
	NetTransform _leftHand;




	void UpdatePlayer()
	{
		if( _head.AmITheOwner() )
		{
			Debug.Assert( _leftHand.AmITheOwner() );

			fjdkal;fjkdls;ajfkdl;sajfkld;sjm
		}
	}




}
*/