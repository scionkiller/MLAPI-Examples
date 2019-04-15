using System.Net;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;


// This is a temporary class to allow us to make a single build for both the server and client
// for prototyping. For production use in OneRoom, we will not have this class (nor the scene
// it is placed in) and will have two separate builds with either the Client or the Server scene
// as the initial scene instead.
public class TemporaryShellScene : MonoBehaviour
{
	static readonly int MARGIN = 10;
	static readonly int BUTTON_WIDTH = 100;
	static readonly int BUTTON_HEIGHT = 20;
	static readonly int LABEL_WIDTH = 800;
	static readonly int LABEL_HEIGHT = 20;
	static readonly int TITLE_WIDTH = 300;
	static readonly int TITLE_HEIGHT = 30;


	[SerializeField]
	string _clientSceneName = null;

	[SerializeField]
	string _serverSceneName = null;


	GUIStyle _titleStyle;


	void Awake()
	{
		_titleStyle = new GUIStyle("BoldLabel");
		_titleStyle.fontSize = 24;
	}

	void OnGUI()
	{
		int yOffset = MARGIN;

		GUI.Label( new Rect( MARGIN, yOffset, TITLE_WIDTH, TITLE_HEIGHT ), "Temporary Shell Scene", _titleStyle );
		yOffset += TITLE_HEIGHT + MARGIN;

		ShowLine( "", ref yOffset );
		ShowLine( "Press one of the buttons below to load either the client or server scene.", ref yOffset );
		ShowLine( "The loaded scene will completely replace this scene.", ref yOffset );
		ShowLine( "", ref yOffset );

		if( GUI.Button(new Rect( MARGIN, yOffset, BUTTON_WIDTH, BUTTON_HEIGHT), "Start client") )
		{
			SceneManager.LoadScene( _clientSceneName, LoadSceneMode.Single );
		}
		yOffset += BUTTON_HEIGHT + MARGIN;

		if( GUI.Button(new Rect( MARGIN, yOffset, BUTTON_WIDTH, BUTTON_HEIGHT), "Start server") )
		{
			SceneManager.LoadScene( _serverSceneName, LoadSceneMode.Single );
		}
		yOffset += BUTTON_HEIGHT + MARGIN;
	}

	void ShowLine( string text, ref int yOffset )
	{
		GUI.Label( new Rect( MARGIN, yOffset, LABEL_WIDTH, LABEL_HEIGHT ), text );
		yOffset += LABEL_HEIGHT + MARGIN;
	}
}