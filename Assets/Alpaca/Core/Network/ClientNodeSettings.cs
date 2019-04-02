using UnityEngine;
using UnityEngine.Networking;


namespace Alpaca
{

public class ClientNodeSettings : MonoBehaviour
{
	// how to contact the server to connect
	public string connectAddress = "127.0.0.1";
}

// ignore Obsolete warning for UNET
#pragma warning disable 618

public class ClientNode : CommonNode
{
	ClientNodeSettings _clientSettings;
	bool _isRunning;


	public bool IsRunning() { return _isRunning; }
	public bool IsConnectedToServer() { return _localIndex.IsValidClientIndex(); }
	public string GetConnectionAddress() { return _clientSettings.connectAddress; }


	public ClientNode( CommonNodeSettings commonSettings, ClientNodeSettings clientSettings ) : base( commonSettings )
	{
		_clientSettings = clientSettings;
		_isRunning = false;

		// TODO: cozeroff
	}

	public override bool Start( out string error )
	{
		ConnectionConfig config = InitializeNetwork( out error );

		// TODO: cozeroff topology here

		_isRunning = true;
		return _isRunning;
	}
}

#pragma warning restore 618

} // namespace Alpaca