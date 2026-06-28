using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;
using SerializationUtility = Sirenix.Serialization.SerializationUtility;

namespace VoidState.InspectorHistory.Editor
{
    // Useful link for icons https://github.com/halak/unity-editor-icons
    public class InspectorHistoryWindow : EditorWindow, IHistoryInteraction
    {
        private const int HISTORY_MAX = 20;
        private const int FREQUENT_MAX = 5;

        [MenuItem("VoidState/Inspector History")]
        public static void OpenWindow()
        {
            var wnd = GetWindow<InspectorHistoryWindow>();
            wnd.titleContent = new GUIContent("Inspector History");
        }

        private List<HistoryEntry> _rawHistory = new List<HistoryEntry>();
        
        private List<HistoryEntry> _rawFavorites = new List<HistoryEntry>();
        private List<HistoryEntry> _rawFrequent = new List<HistoryEntry>();

        /// True if navigation was triggered by this toolset and not a user selecting something.
        /// Used to refrain from updating the history while navigating.
        private bool _navigateFlag = false;

        /// Where we are in the current history. 0 index is most recent, 1 is last, 2 is one before last, etc.
        private int _currentHistoryIndex = 0;

        private NavbarView _navbarView;
        private HistoryListView _favoriteView;
        private HistoryListView _frequentView;
        private HistoryListView _historyView;

        private Vector2 _scrollPosition;

        private bool ShouldDisableBackButton() => _rawHistory.Count <= 1 || _currentHistoryIndex == _rawHistory.Count - 1 || _currentHistoryIndex < 0;
        private bool ShouldDisableForwardButton() => _currentHistoryIndex <= 0;

        protected void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;

            InitializeViews();
            
            LoadHistoryFromEditorPrefs();
        }

        protected void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private void InitializeViews()
        {
            _navbarView ??= new NavbarView(ShouldDisableBackButton, ShouldDisableForwardButton, GoBack, GoForward);
            _frequentView ??= new HistoryListView("Frequent");
            _favoriteView ??= new HistoryListView("Favourites");
            _historyView ??= new HistoryListView("History", HISTORY_MAX / 2);
        }

        private void OnSelectionChanged()
        {
            if (_navigateFlag)
            {
                // Don't update history for internal navigations
                _navigateFlag = false;
                return;
            }

            var activeObject = Selection.activeObject;

            if (activeObject != null)
            {
                if (_currentHistoryIndex > 0)
                {
                    // If you're in the past, we need to drop the old future for the new future
                    _rawHistory.RemoveRange(0, _currentHistoryIndex);
                }

                var historyEntry = _rawHistory.FirstOrDefault(x => x.Value == activeObject) ?? new HistoryEntry(activeObject);

                historyEntry.Uses++;

                _rawHistory.RemoveAll(x => x.Value == activeObject);
                _rawHistory.Insert(0, historyEntry);

                //if (_rawHistory.Count > HISTORY_MAX) _rawHistory.RemoveAt(_rawHistory.Count - 1);

                UpdateFrequent();

                _currentHistoryIndex = 0;
            }
            else
            {
                _currentHistoryIndex = -1;
            }
            Repaint();

            // Save history to EditorPrefs
            SaveHistoryToEditorPrefs();
        }

        public void GoBack()
        {
            if (ShouldDisableBackButton()) return;

            _currentHistoryIndex++;
            _navigateFlag = true;

            Selection.SetActiveObjectWithContext(_rawHistory[_currentHistoryIndex].Value, null);
        }

        public void GoForward()
        {
            if (ShouldDisableForwardButton()) return;

            _currentHistoryIndex--;
            _navigateFlag = true;

            Selection.SetActiveObjectWithContext(_rawHistory[_currentHistoryIndex].Value, null);
        }

        private void OnGUI()
        {
            InitializeViews();

            var selectedEntry = _currentHistoryIndex > -1 && _rawHistory.Count > 0 ? _rawHistory[_currentHistoryIndex] : null;
            
            _navbarView.Draw(_rawHistory, selectedEntry);

            using (var scrollView = new EditorGUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scrollView.scrollPosition;
                
                if (_rawFavorites.Count > 0)
                {
                    _favoriteView.Draw(_rawFavorites, selectedEntry, this);
                }

                if (_rawFrequent.Count > 0)
                {
                    _frequentView.Draw(_rawFrequent, selectedEntry, this);
                }

                _historyView.Draw(_rawHistory, _currentHistoryIndex, this);
            }
            
            Utilities.DrawSeparator();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear")) ClearHistory();
            if (GUILayout.Button("Save")) SaveHistoryToEditorPrefs();
            if (GUILayout.Button("Load")) LoadHistoryFromEditorPrefs();
            EditorGUILayout.EndHorizontal();
        }

        public void SelectHistoryItem(HistoryEntry historyItem)
        {
            int index = _rawHistory.IndexOf(historyItem);
            if (index == _currentHistoryIndex) return;
            
            _currentHistoryIndex = index;
            _navigateFlag = true;
            historyItem.Uses++;
            UpdateFrequent();
            Selection.SetActiveObjectWithContext(historyItem.Value, null);
        }

        public void ToggleFavourite(HistoryEntry historyItem)
        {
            if (_rawFavorites.Contains(historyItem))
            {
                historyItem.IsFavourite = false;
                _rawFavorites.Remove(historyItem);
            }
            else
            {
                historyItem.IsFavourite = true;
                _rawFavorites.Add(historyItem);
            }

            UpdateFrequent();
        }

        private void UpdateFrequent()
        {
            _rawFrequent = _rawHistory.OrderByDescending(x => x.Uses)
                .Where(x => !x.IsFavourite)
                .Take(FREQUENT_MAX).ToList();
        }

        private void ClearHistory()
        {
            _rawFavorites.Clear();
            _rawHistory.Clear();
            _rawFrequent.Clear();
            _currentHistoryIndex = 0;
        }

        private void SaveHistoryToEditorPrefs()
        {
            try
            {
                byte[] serializedData = SerializationUtility.SerializeValue(_rawHistory, DataFormat.Binary);

                // Convert to base64 string for EditorPrefs storage
                string base64String = Convert.ToBase64String(serializedData);
                EditorPrefs.SetString($"{Utilities.PREFS_PREFIX}.history", base64String);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save history to EditorPrefs: {ex.Message}");
            }
        }

        private void LoadHistoryFromEditorPrefs()
        {
            try
            {
                string base64String = EditorPrefs.GetString($"{Utilities.PREFS_PREFIX}.history", "");
                if (!string.IsNullOrEmpty(base64String))
                {
                    // Convert base64 string back to byte array
                    byte[] serializedData = Convert.FromBase64String(base64String);

                    // Deserialize using Sirenix's binary format
                    _rawHistory = SerializationUtility.DeserializeValue<List<HistoryEntry>>(serializedData, DataFormat.Binary);

                    foreach (var entry in _rawHistory)
                    {
                        entry.TryGetReference();
                    }
                    
                    // Initialize favorites list from history items
                    _rawFavorites = _rawHistory.Where(x => x.IsFavourite).ToList();
                    
                    // Initialize frequent list
                    UpdateFrequent();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load history from EditorPrefs: {ex.Message}");
                // Initialize empty lists on failure
                _rawHistory = new List<HistoryEntry>();
                _rawFavorites = new List<HistoryEntry>();
            }
        }
    }
}