﻿using MLAPI;

namespace UnityEditor
{
    [CustomEditor(typeof(TrackedObject), true)]
    [CanEditMultipleObjects]
    public class TrackedObjectEditor : Editor
    {
        private TrackedObject trackedObject;
        private bool initialized;

        private void Init()
        {
            if (initialized)
                return;

            trackedObject = (TrackedObject)target;
            initialized = true;
        }

        public override void OnInspectorGUI()
        {
            Init();
            base.OnInspectorGUI();
            if(NetworkingManager.GetSingleton() != null && NetworkingManager.GetSingleton().IsServer)
            {
                EditorGUILayout.LabelField("Total points: ", trackedObject.TotalPoints.ToString(), EditorStyles.label);
                EditorGUILayout.LabelField("Avg time between points: ", trackedObject.AvgTimeBetweenPointsMs.ToString() + " ms", EditorStyles.label);
                EditorGUILayout.LabelField("Total history: ", trackedObject.TotalTimeHistory.ToString() + " seconds", EditorStyles.label);
            }
            Repaint();
        }
    }
}
