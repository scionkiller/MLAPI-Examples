using System;
using UnityEngine;


// Place on the root of a prefab used to represent the player
public class Avatar : MonoBehaviour
{
	public Transform head;
	public Transform groundRayOrigin;
	// TODO: these two dumb names are because of deprecated fields that still exist on Component
	public Camera avatarCamera;
	public Rigidbody avatarRigidBody;

	public float yawSpeed = 60f;
	public float pitchSpeed = 60f;

	[Range(0f, 90f)]
	public float maxDownwardPitch = 35f;

	[Range(0f, 90f)]
	public float maxUpwardPitch = 85f;
	
	[Tooltip("Meters per second.")]
	public float walkingSpeed = 1.4f; // about 5 km/h
	
	[Tooltip("Meters per second squared.")]
	public float walkingAcceleration = 1.4f;

	[Tooltip("At this distance above the ground, the AvatarController will have no vertical velocity.")]
	public float restHeight = 1.0f;

	[Tooltip("When this far or further from restHeight, the AvatarController will accelerate towards restHeightMaxSpeed to correct it.")]
	public float restHeightDelta = 0.4f;
	
	[Tooltip("Rate at which vertical speed will change when trying to correct AvatarController height.")]
	public float restHeightAcceleration = 10f;
	
	[Tooltip("Maximum vertical speed when trying to correct AvatarController height")]
	public float restHeightMaxSpeed = 10f;

	[Tooltip("Maximum distance the player can interact with Tools")]
	public float interactDistance = 10f;
}

public class AvatarController
{
	Avatar _avatar;
	
	float _currentPitch;
	ToolSettings _lookAtTool;


	public AvatarController( Avatar avatar )
	{
		_avatar = avatar;

		_avatar.avatarCamera.tag = "MainCamera";
		_avatar.avatarCamera.gameObject.GetComponent<AudioListener>().enabled = true;

		OnTeleport();
	}

	public void OnTeleport()
	{	
		// reset lookat
		_currentPitch = 0f;
		_avatar.head.localRotation = Quaternion.identity;
		_lookAtTool = null;
		
		// reset physics
		_avatar.avatarRigidBody.velocity = Vector3.zero;
		_avatar.avatarRigidBody.angularVelocity = Vector3.zero;
	}

	public void Enable()
	{
		_avatar.gameObject.SetActive(true);
	}

	public void Disable()
	{
		_avatar.gameObject.SetActive(false);
	}
	
	public void UpdateAvatar( PlayerInput input )
	{
		float x = input.GetCursorX() * _avatar.yawSpeed * Time.deltaTime;
		float y = input.GetCursorY() * _avatar.pitchSpeed * Time.deltaTime;
		
		// rotate whole avatar for yaw
		_avatar.transform.Rotate( new Vector3(0f, x, 0f), Space.World );
		
		// rotate only head for pitch
		_currentPitch = Mathf.Clamp( _currentPitch - y, -_avatar.maxUpwardPitch, _avatar.maxDownwardPitch );
		_avatar.head.localRotation = Quaternion.Euler( new Vector3( _currentPitch, 0f, 0f ) );

		UpdateLook();
	}
		
	public void FixedUpdateAvatar( PlayerInput input )
	{
		float HEIGHT = _avatar.restHeight;
		float DELTA = _avatar.restHeightDelta;
		float MAX_SPEED = _avatar.restHeightMaxSpeed;
		
		// if we collide with nothing, fall at max speed
		float verticalSign = -1f;
		float verticalTargetSpeed = MAX_SPEED;
		
		// find nearest collision
		Ray r = new Ray( _avatar.groundRayOrigin.position, Vector3.down );
		RaycastHit h;
		if( Physics.Raycast( r, out h, HEIGHT + DELTA ) )
		{
			Debug.Log( String.Format( "ground ray hit: {0} at distance: {1}", h.collider.name, h.distance ) );
			
			float d = h.distance;
			float mag = Mathf.Abs(HEIGHT - d);
			
			// ground too close, move up
			if( d < HEIGHT )
			{
				verticalSign = 1f;
			}
			
			if( mag < 0.01f )
			{
				// close enough, stop moving
				verticalTargetSpeed = 0f;
			}
			else
			{
				verticalTargetSpeed = MAX_SPEED * (mag / DELTA);	
			}
			
			Debug.Log( String.Format( "sign: {0} target speed: {1}", verticalSign, verticalTargetSpeed ) );
		}	
		// could disable key input here if no collision (ie, no "airtouch" if falling)
		// we ignore this because in this game we will probably never be falling
		
		// WALKING UPDATE: AFFECTS X AND Z ONLY
		{
			Vector3 inputDir = new Vector3( input.GetStrafe(), 0, input.GetWalk() );

			float inputMag = inputDir.magnitude;
			if( inputMag > 1f )
			{
				// necessary to avoid going up to sqrt(2) times faster on the diagonal
				inputDir /= inputMag;
			}
			
			Vector3 targetDir = _avatar.transform.TransformDirection(inputDir);
			Vector3 velocity = _avatar.avatarRigidBody.velocity;
			
			Vector3 targetVelocity = targetDir * _avatar.walkingSpeed;
			Vector3 delta = targetVelocity - velocity;
			delta.y = 0f; // we ignore changes in Y
			float deltaMag = delta.magnitude;
			float maxDelta = _avatar.walkingAcceleration * Time.deltaTime;
			if( deltaMag > maxDelta )
			{
				delta = (delta / deltaMag) * maxDelta;
			}
			
			_avatar.avatarRigidBody.AddForce(delta, ForceMode.VelocityChange);
		}
		
		// HEIGHT ADJUSTMENT UPDATE: AFFECTS Y ONLY
		{	
			float velocity = _avatar.avatarRigidBody.velocity.y;
			float targetVelocity = verticalSign * verticalTargetSpeed;
			
			float delta = targetVelocity - velocity;
			float deltaMag = Mathf.Abs(delta);
			
			float maxDelta = _avatar.restHeightAcceleration * Time.deltaTime;
			if( deltaMag > maxDelta )
			{
				delta = (delta / deltaMag) * maxDelta;
			}
			
			_avatar.avatarRigidBody.AddForce( new Vector3(0f, delta, 0f), ForceMode.VelocityChange);
		}
	}


	// PRIVATE

	void UpdateLook()
	{
		Ray r = new Ray( _avatar.head.position, _avatar.head.forward );
		RaycastHit hit;
		if( Physics.Raycast( r, out hit, _avatar.interactDistance, Layer.TOOL_MASK ) )
		{
			_lookAtTool = hit.collider.gameObject.GetComponentInParent<ToolSettings>();
			Debug.Log( String.Format( "Look ray hit, collider is: {0} atom is: {1}", hit.collider.name, ((_lookAtTool != null) ? _lookAtTool.name : "null") ) );
		}
	}
}