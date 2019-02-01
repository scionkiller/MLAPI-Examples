using System.Net;
using System.Collections.Generic;

using UnityEngine;

using MLAPI;
using MLAPI.Data;


public class NetManagerHud : MonoBehaviour
{
	static readonly int MARGIN = 10;
	static readonly int BUTTON_WIDTH = 100;
	static readonly int BUTTON_HEIGHT = 20;
	static readonly int LABEL_WIDTH = 300;
	static readonly int LABEL_HEIGHT = 20;
	static readonly int TITLE_WIDTH = 120;
	static readonly int TITLE_HEIGHT = 30;


	GUIStyle _titleStyle;


	void Awake()
	{
		_titleStyle = new GUIStyle("BoldLabel");
		_titleStyle.fontSize = 24;
	}

    void OnGUI()
    {
		NetworkingManager network = NetworkingManager.singleton;
		int yOffset = MARGIN;

		if( network.isHost )
		{
			ShowTitle( "Host", ref yOffset );
			ShowClientGui( network, ref yOffset );
			ShowServerGui( network, ref yOffset );
		}
		else if( network.isServer )
		{
			ShowTitle( "Server", ref yOffset );
			ShowServerGui( network, ref yOffset );
		}
		else if( network.isClient )
		{
			ShowTitle( "Client", ref yOffset );
			ShowClientGui( network, ref yOffset );
		}
		else
		{
			// we are not anything yet, show buttons to allow user choice
			if( GUI.Button(new Rect( MARGIN, yOffset, BUTTON_WIDTH, BUTTON_HEIGHT), "Start client") )
			{
				network.StartClient();
			}
			yOffset += BUTTON_HEIGHT + MARGIN;

			if( GUI.Button(new Rect( MARGIN, yOffset, BUTTON_WIDTH, BUTTON_HEIGHT), "Start server") )
			{
				network.StartServer();
			}
			yOffset += BUTTON_HEIGHT + MARGIN;

			if( GUI.Button(new Rect( MARGIN, yOffset, BUTTON_WIDTH, BUTTON_HEIGHT), "Start host") )
			{
				network.StartHost();
			}
			yOffset += BUTTON_HEIGHT + MARGIN;
		}
    }

	void ShowTitle( string text, ref int yOffset )
	{
		GUI.Label( new Rect( MARGIN, yOffset, TITLE_WIDTH, TITLE_HEIGHT ), text, _titleStyle );
		yOffset += TITLE_HEIGHT + MARGIN;
	}

	void ShowLine( string text, ref int yOffset )
	{
		GUI.Label( new Rect( MARGIN, yOffset, LABEL_WIDTH, LABEL_HEIGHT ), text );
		yOffset += LABEL_HEIGHT + MARGIN;
	}

	void ShowServerGui( NetworkingManager network, ref int yOffset )
	{
		Dictionary<uint, NetworkedClient> clients = network.ConnectedClients;
		ShowLine( "Network time: " + network.NetworkTime, ref yOffset );
		ShowLine( "Connected client count: " + clients.Count, ref yOffset );

		foreach( KeyValuePair<uint, NetworkedClient> p in clients )
		{
			ShowLine( "Key: " + p.Key + " Client.id: " + p.Value.ClientId, ref yOffset );
		}
	}

	void ShowClientGui( NetworkingManager network, ref int yOffset )
	{
		ShowLine( "Am I connected? " + (network.IsConnectedClient ? "Yes" : "No"), ref yOffset );
		ShowLine( "My Client.id: " + network.LocalClientId, ref yOffset );
	}
}