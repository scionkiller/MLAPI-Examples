using System.Collections;
using System.Collections.Generic;
using MLAPI.MonoBehaviours.Core;
using MLAPI.NetworkingManagerComponents.Binary;
using UnityEngine;

public class ColorRandomizer : NetworkedBehaviour
{
    public MeshRenderer meshRenderer;

    public override void NetworkStart()
    {
        if (isClient)
            RegisterMessageHandler("OnChangeColor", OnChangeColor);
    }

    private void OnChangeColor(uint clientId, byte[] data)
    {
        BitReader reader = new BitReader(data);
        float r = reader.ReadFloat();
        float g = reader.ReadFloat();
        float b = reader.ReadFloat();
        meshRenderer.material.color = new Color(r, g, b);
    }

    private void OnGUI()
    {
        if (isServer)
        {
            if (GUI.Button(new Rect(200, 25, 200, 20), "Set random plane color"))
            {
                Color color = Random.ColorHSV();
                meshRenderer.material.color = color;
                using (BitWriter writer = new BitWriter())
                {
                    writer.WriteFloat(color.r);
                    writer.WriteFloat(color.g);
                    writer.WriteFloat(color.b);
                    SendToClientsTarget("OnChangeColor", "ColorChannel", writer.Finalize());
                }
            }
        }
    }

}
