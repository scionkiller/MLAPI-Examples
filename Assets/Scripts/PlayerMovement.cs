using MLAPI;
using MLAPI.MonoBehaviours.Core;
using MLAPI.NetworkingManagerComponents;
using MLAPI.NetworkingManagerComponents.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : NetworkedBehaviour
{
    void Update ()
    {
		if(isLocalPlayer)
        {
            transform.Translate(new Vector3(Input.GetAxis("Horizontal") * 2f * Time.deltaTime, 0, Input.GetAxis("Vertical") * 2f * Time.deltaTime));
		}
	}
}
