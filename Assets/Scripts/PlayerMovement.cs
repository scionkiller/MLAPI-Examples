using MLAPI;
using UnityEngine;

public class PlayerMovement : NetworkedBehaviour
{
    void Update ()
    {
		if( IsLocalPlayer )
        {
            transform.Translate(new Vector3(Input.GetAxis("Horizontal") * 2f * Time.deltaTime, 0, Input.GetAxis("Vertical") * 2f * Time.deltaTime));
		}
	}
}
