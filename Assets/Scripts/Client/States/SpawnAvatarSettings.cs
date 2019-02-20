using UnityEngine;
using UnityEngine.SceneManagement;

using TMPro;
using MLAPI;
using MLAPI.Serialization;

public class SpawnAvatarSettings : ClientStateSettings
{
    public TMP_Text display;
}

public class SpawnAvatar : ClientState
{
    ClientWorld _world;
    SpawnAvatarSettings _settings;
    ClientStateId _transitionState;


    #region ClientState interface (including FsmState interface)

    public void Initialize(ClientWorld world, ClientStateSettings settings)
    {
        _world = world;
        _settings = (SpawnAvatarSettings)settings;
        _settings.Hide();
        _transitionState = ClientStateId.NO_TRANSITION;
    }

    public void OnEnter()
    {
        _settings.Show();
        _settings.display.text = "Sent spawn request to server...";

        using (PooledBitStream stream = PooledBitStream.Get())
        {
            BitWriter writer = new BitWriter(stream);
            writer.WriteByte((byte)MessageType.SpawnAvatarRequest);

            NetworkingManager networking = _world.GetNetwork();
            networking.SendCustomMessage(networking.ServerClientId, stream);
        }
    }

    public void OnExit()
    {
        _settings.Hide();
    }

    public void OnUpdate()
    {
        // TODO
    }

    public void OnFixedUpdate() { }

    public int GetTransitionID() { return (int)_transitionState; }
    public int GetID() { return (int)ClientStateId.SpawnAvatar; }

    #endregion // ClientState interface
}