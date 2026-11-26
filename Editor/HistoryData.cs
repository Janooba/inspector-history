using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VoidState.InspectorHistory.Editor
{
    /// Saved outside the Assets folder, so that saving does not cause an asset import.
    [FilePath("UserSettings/InspectorHistory.asset", FilePathAttribute.Location.ProjectFolder)]
    public class HistoryData : ScriptableSingleton<HistoryData>
    {
        public const string SETTINGS_PATH = "Preferences/Inspector History";

        private static SerializedObject _settingsObject;

        public int maxHistoryDisplayed = 10;
        public int MaxHistoryStored => maxHistoryDisplayed * 2;

        public bool showSceneObjectsSeparately = true;
        public bool showUnloaded = false;
        [Tooltip("Ignore folders that are selected in the Project window.")]
        public bool skipFolders = false;
        public bool showDebug = false;

        [HideInInspector, SerializeField]
        public List<HistoryEntry> history = new List<HistoryEntry>();

        [HideInInspector, SerializeField]
        public List<HistoryEntry> favourites = new List<HistoryEntry>();

        public void SaveToFile()
        {
            Save(true);
        }

        /// <summary> Trims the history to it's max stored length. </summary>
        /// <returns> How many entries were removed. </returns>
        public int Trim()
        {
            if (history.Count > MaxHistoryStored)
            {
                int toRemove = history.Count - MaxHistoryStored;
                history.RemoveRange(MaxHistoryStored, toRemove);
                return toRemove;
            }

            return 0;
        }

        [SettingsProvider]
        private static SettingsProvider CreateSettingsProvider()
        {
            return new SettingsProvider(SETTINGS_PATH, SettingsScope.User)
            {
                guiHandler = _ => DrawSettings(),
                keywords = new[] { "history", "favourites", "selection" }
            };
        }

        private static void DrawSettings()
        {
            if (_settingsObject == null || _settingsObject.targetObject != instance)
                _settingsObject = new SerializedObject(instance);

            _settingsObject.Update();
            EditorGUI.BeginChangeCheck();

            var property = _settingsObject.GetIterator();
            property.NextVisible(true); // Skip m_Script
            while (property.NextVisible(false))
            {
                EditorGUILayout.PropertyField(property, true);
            }

            if (EditorGUI.EndChangeCheck())
            {
                _settingsObject.ApplyModifiedProperties();
                instance.SaveToFile();
            }

            DrawDebugPanel();
        }

        private static void DrawDebugPanel()
        {
            if (!instance.showDebug) return;

            var service = HistoryService.Instance;

            GUILayout.Label("Debug Information", EditorStyles.largeLabel);

            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.PropertyField(_settingsObject.FindProperty(nameof(history)), true);
                EditorGUILayout.PropertyField(_settingsObject.FindProperty(nameof(favourites)), true);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear")) service.ClearHistory();
            if (GUILayout.Button("Save")) service.SaveHistory();
            if (GUILayout.Button("Load")) service.LoadHistory();
            EditorGUILayout.EndHorizontal();
        }
    }
}
