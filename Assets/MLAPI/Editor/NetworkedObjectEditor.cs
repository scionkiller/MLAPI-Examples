#pragma warning disable 618
using Alpaca;
using UnityEngine;

namespace UnityEditor
{
    [CustomEditor(typeof(NetworkedObject), true)]
    [CanEditMultipleObjects]
    public class NetworkedObjectEditor : Editor
    {
        private bool initialized;
        private NetworkedObject networkedObject;

        private void Init()
        {
            if (initialized)
                return;
            initialized = true;
            networkedObject = (NetworkedObject)target;
        }

        public override void OnInspectorGUI()
        {
            Init();

			NetworkingManager network = NetworkingManager.GetSingleton();

            if( network == null || (!network.IsServer && !network.IsClient))
                base.OnInspectorGUI(); // Only do default GUI if we are not running. This is where the ServerOnly box is drawn

            if( !networkedObject.IsSpawned && network != null && network.IsServer)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Spawn", "Spawns the object across the network"));
                if (GUILayout.Toggle(false, "Spawn", EditorStyles.miniButtonLeft))
                {
                    networkedObject.Spawn();
                    EditorUtility.SetDirty(target);
                }
                EditorGUILayout.EndHorizontal();
            }
            else if (networkedObject.IsSpawned)
            {
                EditorGUILayout.LabelField("PrefabName: ", networkedObject.NetworkedPrefabName, EditorStyles.label);
                EditorGUILayout.LabelField("PrefabHash: ", networkedObject.NetworkedPrefabHash.ToString(), EditorStyles.label);
                EditorGUILayout.LabelField("NetworkId: ", networkedObject.NetworkId.ToString(), EditorStyles.label);
                EditorGUILayout.LabelField("OwnerId: ", networkedObject.OwnerClientId.ToString(), EditorStyles.label);
                EditorGUILayout.LabelField("isSpawned: ", networkedObject.IsSpawned.ToString(), EditorStyles.label);
                EditorGUILayout.LabelField("isLocalPlayer: ", networkedObject.IsLocalPlayer.ToString(), EditorStyles.label);
                EditorGUILayout.LabelField("isOwner: ", networkedObject.IsOwner.ToString(), EditorStyles.label);
				EditorGUILayout.LabelField("isOwnedByServer: ", networkedObject.IsOwnedByServer.ToString(), EditorStyles.label);
                EditorGUILayout.LabelField("isPoolObject: ", networkedObject.IsPooledObject.ToString(), EditorStyles.label);
                EditorGUILayout.LabelField("isPlayerObject: ", networkedObject.IsPlayerObject.ToString(), EditorStyles.label);
            }
        }
    }
}
#pragma warning restore 618
