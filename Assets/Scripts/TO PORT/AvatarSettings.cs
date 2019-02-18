using UnityEngine;


// Place on the root of a prefab used to represent the player
public class AvatarSettings : MonoBehaviour
{
	public Transform head;
	public Transform groundRayOrigin;
	// TODO: these two dumb names are because of deprecated fields that still exist on Component
	public Camera avatarCamera;
	public Rigidbody avatarRigidBody;
	public float yawSpeed;
	public float pitchSpeed;	

	[Range(0f, 90f)]
	public float maxDownwardPitch = 35f;

	[Range(0f, 90f)]
	public float maxUpwardPitch = 85f;
	
	[Tooltip("Meters per second.")]
	public float walkingSpeed = 1.4f; // about 5 km/h
	
	[Tooltip("Meters per second squared.")]
	public float walkingAcceleration = 1.4f;

	[Tooltip("At this distance above the ground, the avatar will have no vertical velocity.")]
	public float restHeight = 1.0f;

	[Tooltip("When this far or further from restHeight, the avatar will accelerate towards restHeightMaxSpeed to correct it.")]
	public float restHeightDelta = 0.4f;
	
	[Tooltip("Rate at which vertical speed will change when trying to correct avatar height.")]
	public float restHeightAcceleration = 10f;
	
	[Tooltip("Maximum vertical speed when trying to correct avatar height")]
	public float restHeightMaxSpeed = 10f;
}