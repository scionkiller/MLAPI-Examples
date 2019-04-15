using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using Alpaca;

/*

[CustomEditor(typeof(AlpacaNetwork), true)]
[CanEditMultipleObjects]
public class NetworkingManagerEditor : Editor
{
	private SerializedProperty LogLevelProperty;
	private SerializedProperty NetworkConfigProperty;

	private ReorderableList networkPrefabsList;
	private ReorderableList channelsList;
	private ReorderableList registeredScenesList;

	private AlpacaNetwork network;
	private bool initialized;


	private void Init()
	{
		if (initialized)
			return;

		initialized = true;
		network = (AlpacaNetwork)target;
		LogLevelProperty = serializedObject.FindProperty("LogLevel");
		NetworkConfigProperty = serializedObject.FindProperty("config");
	}

	private void CheckNullProperties()
	{
		if( LogLevelProperty == null )
			LogLevelProperty = serializedObject.FindProperty("LogLevel");
		if( NetworkConfigProperty == null )
			NetworkConfigProperty = serializedObject.FindProperty("config");
	}

	private void OnEnable()
	{
		networkPrefabsList = new ReorderableList(serializedObject, serializedObject.FindProperty("config").FindPropertyRelative("NetworkedPrefabs"), true, true, true, true);
		networkPrefabsList.drawHeaderCallback = (Rect rect) => {
			EditorGUI.LabelField(rect, "NetworkedPrefabs");
		};
		networkPrefabsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
		{
			SerializedProperty element = networkPrefabsList.serializedProperty.GetArrayElementAtIndex(index);
			int firstLabelWidth = 50;

			EditorGUI.LabelField(new Rect(rect.x, rect.y, firstLabelWidth, EditorGUIUtility.singleLineHeight), "Prefab");
			EditorGUI.PropertyField(new Rect(rect.x + firstLabelWidth, rect.y, rect.width - firstLabelWidth,
				EditorGUIUtility.singleLineHeight), element, GUIContent.none);
		};


		channelsList = new ReorderableList(serializedObject, serializedObject.FindProperty("config").FindPropertyRelative("Channels"), true, true, true, true);
		channelsList.drawHeaderCallback = (Rect rect) => {
			EditorGUI.LabelField(rect, "Channels");
		};
		channelsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
		{
			SerializedProperty element = channelsList.serializedProperty.GetArrayElementAtIndex(index);


			int firstLabelWidth = 50;
			int secondLabelWidth = 40;
			int secondFieldWidth = 150;
			int reduceFirstWidth = 45;

			EditorGUI.LabelField(new Rect(rect.x, rect.y, firstLabelWidth, EditorGUIUtility.singleLineHeight), "Name");
			EditorGUI.PropertyField(new Rect(rect.x + firstLabelWidth, rect.y, rect.width - firstLabelWidth - secondLabelWidth - secondFieldWidth - reduceFirstWidth,
				EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("Name"), GUIContent.none);


			EditorGUI.LabelField(new Rect(rect.width - secondLabelWidth - secondFieldWidth, rect.y, secondLabelWidth, EditorGUIUtility.singleLineHeight), "Type");
			EditorGUI.PropertyField(new Rect(rect.width - secondFieldWidth, rect.y, secondFieldWidth,
				EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("Type"), GUIContent.none);
		};


		registeredScenesList = new ReorderableList(serializedObject, serializedObject.FindProperty("config").FindPropertyRelative("RegisteredScenes"), true, true, true, true);
		registeredScenesList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
		{
			SerializedProperty element = registeredScenesList.serializedProperty.GetArrayElementAtIndex(index);
			int firstLabelWidth = 50;
			int padding = 20;

			EditorGUI.LabelField(new Rect(rect.x, rect.y, firstLabelWidth, EditorGUIUtility.singleLineHeight), "Name");
			EditorGUI.PropertyField(new Rect(rect.x + firstLabelWidth, rect.y, rect.width - firstLabelWidth - padding,
				EditorGUIUtility.singleLineHeight), element, GUIContent.none);

		};

		registeredScenesList.drawHeaderCallback = (Rect rect) => {
			EditorGUI.LabelField(rect, "Registered Scene Names");
		};
	}

	public override void OnInspectorGUI()
	{
		Init();
		CheckNullProperties();
		if (!network.IsServer && !network.IsClient)
		{
			serializedObject.Update();
			EditorGUILayout.PropertyField(LogLevelProperty);

			EditorGUILayout.Space();
			networkPrefabsList.DoLayoutList();
			
			EditorGUILayout.Space();
			channelsList.DoLayoutList();
			EditorGUILayout.Space();

			serializedObject.ApplyModifiedProperties();
			base.OnInspectorGUI();
		}
	}
}

*/
