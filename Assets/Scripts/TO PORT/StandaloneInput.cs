using UnityEngine;

public class StandaloneInput
{
	public float GetAxis(string name )
	{
		return Input.GetAxis(name);
	}


	public bool GetButton(string name)
	{
		return Input.GetButton(name);
	}


	public bool GetButtonDown(string name)
	{
		return Input.GetButtonDown(name);
	}


	public bool GetButtonUp(string name)
	{
		return Input.GetButtonUp(name);
	}


	public Vector3 MousePosition()
	{
		return Input.mousePosition;
	}
}