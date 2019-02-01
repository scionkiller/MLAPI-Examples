using MLAPI;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTest : NetworkedBehaviour
{
    // THis doesn't exist anymore I think?
    //[SyncedVar]
    public string MySyncedName;
    public Text TextField;
    public GameObject cubePrefab;
    public GameObject spherePrefab;
    public Material planeMaterial;

    public override void NetworkStart()
    {
        if (isServer && isLocalPlayer)
            MySyncedName = "SyncVarTest: " + Random.Range(50, 10000);
    }

    private void OnGUI()
    {
        int y = 25;
        if (isServer && isLocalPlayer)
        {
            y += 25;
            if (GUI.Button(new Rect(200, y, 200, 20), "Change Text with SyncVar"))
            {
                MySyncedName = "SyncVarTest: " + Random.Range(50, 10000);
            }

            y += 25;
            if (GUI.Button(new Rect(200, y, 200, 20), "Spawn cube"))
            {
                GameObject go = Instantiate(cubePrefab);
                go.transform.position = transform.position + new Vector3(0, 3f, 0);
                go.GetComponent<NetworkedObject>().Spawn();
            }

            y += 25;
            if (GUI.Button(new Rect(200, y, 200, 20), "Spawn sphere"))
            {
                GameObject go = Instantiate(spherePrefab);
                go.transform.position = transform.position + new Vector3(0, 3f, 0);
                go.GetComponent<NetworkedObject>().Spawn();
            }
        }
    }

    private void Update()
    {
        TextField.text = MySyncedName;
    }

}
