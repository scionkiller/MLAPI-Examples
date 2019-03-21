using System;
using UnityEngine;

// Maps physical inputs into logical player actions
// PlayerInput should therefore never have XButton or MouseMove,
// but rather JumpButton or Look.
public class PlayerInput
{
	// axes
	float _cursorX;
	float _cursorY;
	float _walk;
	float _strafe;

	// instantaneous button clicks
	bool _quitGame;
	bool _interact;



	public void UpdateInput()
	{
		_cursorX = Input.GetAxis("Mouse X");
		_cursorY = Input.GetAxis("Mouse Y");
		_walk = Input.GetAxis("Vertical");
		_strafe = Input.GetAxis("Horizontal");

		_quitGame = Input.GetKeyUp( KeyCode.Escape );
		_interact = Input.GetMouseButtonUp( 0 );

		//Debug.Log( String.Format( "UpdateInput - x: {0} y: {1} walk: {2} strafe: {3}", _cursorX, _cursorY, _walk, _strafe ) );
	}

	public float GetCursorX() { return _cursorX; }
	public float GetCursorY() { return _cursorY; }
	public float GetWalk()  { return _walk; }
	public float GetStrafe() { return _strafe; }

	public bool DidQuit() { return _quitGame; }
	public bool DidInteract() { return _interact; }
}