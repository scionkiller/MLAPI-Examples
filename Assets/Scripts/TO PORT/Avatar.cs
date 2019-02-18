using UnityEngine;


public class Avatar
{
	/*
	AvatarSettings _settings;
	float _currentPitch;


	public Action<byte[], uint, ConnectionApprovedDelegate> ConnectionApprovalCallback = null;

	static public Avatar SpawnAvatar( GameObject prefab, Vector3 position )
	{

	}

	public Avatar( AvatarSettings settings )
	{
		_settings = settings;

		_settings.camera.tag = "MainCamera";
		_settings.camera.gameObject.GetComponent<AudioListener>().enabled = true;

		OnTeleport();
	}

	public void OnTeleport()
	{	
		// reset lookat
		_currentPitch = 0f;
		_head.localRotation = Quaternion.identity;
		_lookAtomGuid = Atom.INVALID;
		
		// reset physics
		_rigidBody.velocity = Vector3.zero;
		_rigidBody.angularVelocity = Vector3.zero;
	}

	public void Enable()
	{
		_avatar.gameObject.SetActive(true);
	}

	public void Disable()
	{
		_avatar.gameObject.SetActive(false);
	}
	
	public void UpdateAvatar( PlayerInput input, bool allowMoveAndLook )
	{
		if( allowMoveAndLook )
		{
			float x = input.GetCursorX() * _playingSettings.GetYawSpeed() * Time.deltaTime;
			float y = input.GetCursorY() * _playingSettings.GetPitchSpeed() * Time.deltaTime;
			
			// rotate whole avatar for yaw
			_avatar.Rotate( new Vector3(0f, x, 0f), Space.World );
			
			// rotate only head for pitch
			_currentPitch = Mathf.Clamp( _currentPitch - y, -_playingSettings.GetMaxUpwardPitch(), _playingSettings.GetMaxDownwardPitch() );
			_head.localRotation = Quaternion.Euler( new Vector3( _currentPitch, 0f, 0f ) );
		}

		UpdateLook( allowMoveAndLook );
	}
		
	public void FixedUpdateAvatar( PlayerInput input, bool allowMoveAndLook )
	{
		float HEIGHT = _playingSettings.GetRestHeight();
		float DELTA = _playingSettings.GetRestHeightDelta();
		float MAX_SPEED = _playingSettings.GetRestHeightMaxSpeed();
		
		// if we collide with nothing, fall at max speed
		float verticalSign = -1f;
		float verticalTargetSpeed = MAX_SPEED;
		
		// find nearest collision
		Ray r = new Ray( _groundRayOrigin.position, Vector3.down );
		Debug.DrawRay( r.origin, r.direction * (HEIGHT+DELTA) );
		RaycastHit h;
		if( Physics.Raycast( r, out h, HEIGHT + DELTA ) )
		{
			//Log.Info( "ray origin x: @ y: @ z: @", Log.args + _avatar.position.x + _avatar.position.y + _avatar.position.z );
			Log.Info( "ground ray hit: @ at distance: @", Log.args + h.collider.name + h.distance );
			
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
			
			//Log.Info( "sign: @ target speed: @", Log.args +  verticalSign + verticalTargetSpeed );
		}	
		// could disable key input here if no collision (ie, no "airtouch" if falling)
		// we ignore this because in this game we will probably never be falling
		
		// WALKING UPDATE: AFFECTS X AND Z ONLY
		{
			Vector3 inputDir = Vector3.zero;
			if( allowMoveAndLook )
			{
				inputDir = new Vector3( input.GetStrafe(), 0, input.GetWalk() );
			}

			float inputMag = inputDir.magnitude;
			if( inputMag > 1f )
			{
				// necessary to avoid going up to sqrt(2) times faster on the diagonal
				inputDir /= inputMag;
			}
			
			Vector3 targetDir = _avatar.TransformDirection(inputDir);
			Vector3 velocity = _rigidBody.velocity;
			
			Vector3 targetVelocity = targetDir * _playingSettings.GetWalkingSpeed();
			Vector3 delta = targetVelocity - velocity;
			delta.y = 0f; // we ignore changes in Y
			float deltaMag = delta.magnitude;
			float maxDelta = _playingSettings.GetWalkingAcceleration() * Time.deltaTime;
			if( deltaMag > maxDelta )
			{
				delta = (delta / deltaMag) * maxDelta;
			}
			
			_rigidBody.AddForce(delta, ForceMode.VelocityChange);
		}
		
		// HEIGHT ADJUSTMENT UPDATE: AFFECTS Y ONLY
		{	
			float velocity = _rigidBody.velocity.y;
			float targetVelocity = verticalSign * verticalTargetSpeed;
			
			float delta = targetVelocity - velocity;
			float deltaMag = Mathf.Abs(delta);
			
			float maxDelta = _playingSettings.GetRestHeightAcceleration() * Time.deltaTime;
			if( deltaMag > maxDelta )
			{
				delta = (delta / deltaMag) * maxDelta;
			}
			
			_rigidBody.AddForce( new Vector3(0f, delta, 0f), ForceMode.VelocityChange);
		}
	}


	// PRIVATE

	void UpdateLook( bool allowLook )
	{
		Atom atom = null;
		Ray r = GetLookRay();
		RaycastHit hit;
		if( allowLook && Physics.Raycast( r, out hit, _playingSettings.GetInteractDistance(), Layer.ONLY_ITEM_MASK ) )
		{
			atom = hit.collider.gameObject.GetComponentInParent<Atom>();
			Log.Trace( "Look ray hit, collider is: @ atom is: @", Log.args + hit.collider.name + ((atom != null) ? atom.name : "null") );
		}
		
		if( atom == null )
		{
			_lookAtomGuid = Atom.INVALID;
		}
		else
		{
			_lookAtomGuid = atom.GetGuidIndex();
		}
	}
	*/
}