using UnityEngine;


namespace Alpaca
{

public class ClientNodeSettings : MonoBehaviour
{
	// how to contact the server to connect
	public string connectAddress = "127.0.0.1";
}

public class ClientNode
{
	CommonNodeSettings _commonSettings;
	ClientNodeSettings _clientSettings;


	// A synchronized time, represents the time in seconds since the server started. Replicated across all nodes.
	float _networkTime;

	// NodeId that identifies us
	NodeId _localNodeId;

	// storage for a single message of max size
	byte[] _messageBuffer;





	public NodeId GetLocalNodeId() { return _localNodeId; }
	public float GetNetworkTime() { return _networkTime; }
	public bool IsConnectedToServer() { return _localNodeId != NodeId.INVALID_NODE_ID; }


	public ClientNode( CommonNodeSettings commonSettings, ClientNodeSettings clientSettings )
	{
		_commonSettings = commonSettings;
		_clientSettings = clientSettings;

		// network time and node id will be set properly when we connect to the server
		_networkTime = -1f;
		_localNodeId = NodeId.INVALID_NODE_ID;
		_connectedToServer = false;

		_messageBuffer = new byte[_commonSettings.messageBufferSize];

		// TODO: cozeroff
	}
}

} // namespace Alpaca