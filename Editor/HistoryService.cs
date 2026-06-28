using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Sirenix.Serialization;
using SerializationUtility = Sirenix.Serialization.SerializationUtility;

namespace VoidState.InspectorHistory.Editor
{
    [InitializeOnLoad]
    public class HistoryService : IDisposable
    {
        private static HistoryService _instance;
        public static HistoryService Instance
        {
            get
            {
                _instance ??= new HistoryService();
                return _instance;
            }
        }
        
        static HistoryService()
        {
            _instance = new HistoryService();
        }
        
        private const int FREQUENT_MAX = 5;
        
        /// True if navigation was triggered by this toolset and not a user selecting something.
        /// Used to refrain from updating the history while navigating.
        private bool _navigateFlag = false;

        /// Where we are in the current history. 0 index is most recent, 1 is last, 2 is one before last, etc.
        private int _currentHistoryIndex = 0;
        
        public HistoryEntry SelectedEntry => _currentHistoryIndex > -1 && _rawHistory.Count > 0 ? _rawHistory[_currentHistoryIndex] : null;
        
        // History
        private List<HistoryEntry> _rawHistory = new List<HistoryEntry>();
        public List<HistoryEntry> HistoryEntries => _rawHistory;
        
        // Favourites
        private List<HistoryEntry> _rawFavourites = new List<HistoryEntry>();
        public List<HistoryEntry> FavouriteEntries => _rawFavourites;
        
        // Frequents
        private List<HistoryEntry> _rawFrequent = new List<HistoryEntry>();
        public List<HistoryEntry> FrequentEntries => _rawFrequent;
        
        public bool CanGoBack => _rawHistory.Count > 1 && _currentHistoryIndex < _rawHistory.Count - 1 && _currentHistoryIndex >= 0;
        public bool CanGoForward => _currentHistoryIndex >= 0;
        
        public HistoryService()
        {
            LoadHistoryFromEditorPrefs();
            Selection.selectionChanged += OnSelectionChanged;
        }
        
        public void Dispose()
        {
            SaveHistoryToEditorPrefs();
            Selection.selectionChanged -= OnSelectionChanged;
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

                UpdateFrequent();

                _currentHistoryIndex = 0;
            }
            else
            {
                _currentHistoryIndex = -1;
            }

            // Save history to EditorPrefs
            SaveHistoryToEditorPrefs();
        }
        
        private void UpdateFrequent()
        {
            _rawFrequent = _rawHistory.OrderByDescending(x => x.Uses)
                .Where(x => !x.IsFavourite)
                .Take(FREQUENT_MAX).ToList();
        }
        
        #region Action Callbacks
        public void GoBack()
        {
            if (!CanGoBack) return;

            _currentHistoryIndex++;
            _navigateFlag = true;

            Selection.SetActiveObjectWithContext(_rawHistory[_currentHistoryIndex].Value, null);
        }

        public void GoForward()
        {
            if (!CanGoForward) return;

            _currentHistoryIndex--;
            _navigateFlag = true;

            Selection.SetActiveObjectWithContext(_rawHistory[_currentHistoryIndex].Value, null);
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
            if (_rawFavourites.Contains(historyItem))
            {
                historyItem.IsFavourite = false;
                _rawFavourites.Remove(historyItem);
            }
            else
            {
                historyItem.IsFavourite = true;
                _rawFavourites.Add(historyItem);
            }

            UpdateFrequent();
        }
        #endregion
        
        #region Save / Load
        public void SaveHistoryToEditorPrefs()
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

        public void LoadHistoryFromEditorPrefs()
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
                    _rawFavourites = _rawHistory.Where(x => x.IsFavourite).ToList();
                    
                    // Initialize frequent list
                    UpdateFrequent();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load history from EditorPrefs: {ex.Message}");
                // Initialize empty lists on failure
                _rawHistory = new List<HistoryEntry>();
                _rawFavourites = new List<HistoryEntry>();
            }
        }
        
        public void ClearHistory()
        {
            _rawFavourites.Clear();
            _rawHistory.Clear();
            _rawFrequent.Clear();
            _currentHistoryIndex = 0;
        }
        #endregion
    }
}
