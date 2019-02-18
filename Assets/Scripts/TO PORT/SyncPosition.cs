using MLAPI;
using System.IO;
using UnityEngine;

public class SyncPosition : NetworkedBehaviour
{
    private float lastSentTime;
    public float PosUpdatesPerSecond = 20;


    private void Update()
    {
        if( !IsLocalPlayer )
		{
    		return;
		}
		
        if(Time.time - lastSentTime > (1f / PosUpdatesPerSecond))
        {
            using(MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(transform.position.x);
                    writer.Write(transform.position.y);
                    writer.Write(transform.position.z);
                }
                InvokeServerRpc(OnRecievePositionUpdate, stream);
            }
            lastSentTime = Time.time;
        }
        transform.Translate(new Vector3(Input.GetAxis("Horizontal") * Time.deltaTime, Input.GetAxis("Vertical") * Time.deltaTime, 0));
    }

    //This gets called on all clients except the one the position update is about.
    [ClientRPC]
    void OnSetClientPosition(uint clientId, Stream stream)
    {
        using (BinaryReader reader = new BinaryReader(stream))
        {
            uint targetNetId = reader.ReadUInt32();
            if (targetNetId != NetworkId)
                return;
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            GetNetworkedObject(targetNetId).transform.position = new Vector3(x, y, z);
        }
    }

    //This gets called on the server when a client sends it's position.
    [ServerRPC]
    void OnRecievePositionUpdate(uint clientId, Stream stream)
    {
        //This makes it behave like a HLAPI Command. It's only invoked on the same object that called it.
        if (clientId != OwnerClientId)
            return;
        using (BinaryReader reader = new BinaryReader(stream))
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            transform.position = new Vector3(x, y, z);
        }
        using (MemoryStream writeStream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(writeStream))
            {
                writer.Write(NetworkId);
                writer.Write(transform.position.x);
                writer.Write(transform.position.y);
                writer.Write(transform.position.z);
            }
            //Sends the position to all clients except the one who requested it. Similar to a Rpc with a if(isLocalPlayer) return;
            InvokeClientRpcOnEveryoneExcept(OnSetClientPosition, clientId, writeStream);
        }
    }
}
