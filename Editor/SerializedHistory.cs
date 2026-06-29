using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VoidState.InspectorHistory.Editor
{
    public class SerializedHistory : ScriptableObject
    {
        private static SerializedHistory _instance;
        public static SerializedHistory Instance
        {
            get
            {
                if (!_instance)
                {
                    var guids = AssetDatabase.FindAssetGUIDs("t:SerializedHistory");
                    if (guids.Length == 0)
                    {
                        // None found, we must create it
                        _instance = CreateInstance<SerializedHistory>();
                        if (!AssetDatabase.IsValidFolder($"Assets/Resources"))
                            AssetDatabase.CreateFolder("Assets", "Resources");
                        AssetDatabase.CreateAsset(_instance, $"Assets/Resources/SelectionHistoryConfig.asset");
                        AssetDatabase.SaveAssets();
                    }
                    else
                    {
                        if (guids.Length > 1)
                        {
                            // We have more than one? That could cause issues.
                            Debug.LogError(
                                $"There is more than one serialized history asset in this project. This could" +
                                $"cause weird issues and inconsistencies. Please ensure only one exists.");
                        }
                        
                        _instance = AssetDatabase.LoadAssetByGUID<SerializedHistory>(guids[0]);
                    }
                }

                return _instance;
            }
        }

        public bool showDebug = false;
        
        [HideInInspector]
        public List<HistoryEntry> history = new List<HistoryEntry>();

        [CustomEditor(typeof(SerializedHistory))]
        public class SerializedHistoryEditor : UnityEditor.Editor
        {
            private SerializedHistory _serializedHistory;
            private SerializedProperty _serializedHistoryListProperty;

            private void OnEnable()
            {
                _serializedHistoryListProperty = serializedObject.FindProperty(nameof(history));
            }

            public override void OnInspectorGUI()
            {
                _serializedHistory ??= target as SerializedHistory;
                base.OnInspectorGUI();
                
                DrawDebugPanel();
            }

            private void DrawDebugPanel()
            {
                if (!_serializedHistory.showDebug) return;
                
                var service = HistoryService.Instance;
                
                GUILayout.Label("Debug Information", EditorStyles.largeLabel);
                
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUILayout.PropertyField(_serializedHistoryListProperty, true);
                }
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Clear")) service.ClearHistory();
                if (GUILayout.Button("Save")) service.SaveHistoryToAsset();
                if (GUILayout.Button("Load")) service.LoadHistoryFromAsset();
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
