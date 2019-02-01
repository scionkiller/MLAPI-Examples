using System.Collections;
using System.Collections.Generic;
using System.IO;
using MLAPI;
using MLAPI.Serialization;
using UnityEngine;

public class ColorRandomizer : NetworkedBehaviour
{
    public MeshRenderer meshRenderer;

    [ClientRPC]
    private void OnChangeColor(uint clientId, Stream stream)
    {
        BitReader reader = new BitReader(stream);
        float r = reader.ReadSingle();
        float g = reader.ReadSingle();
        float b = reader.ReadSingle();
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
                using (MemoryStream stream = new MemoryStream())
                {
                    BitWriter writer = new BitWriter(stream);
                    writer.WriteSingle(color.r);
                    writer.WriteSingle(color.g);
                    writer.WriteSingle(color.b);
                    InvokeClientRpcOnEveryone(OnChangeColor, stream);
                }
            }
        }
    }

}
