using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

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
        
        /// True if navigation was triggered by this toolset and not a user selecting something.
        /// Used to refrain from updating the history while navigating.
        private bool _navigateFlag = false;

        /// Where we are in the current history. 0 index is most recent, 1 is last, 2 is one before last, etc.
        private int _currentHistoryIndex = 0;
        
        public HistoryEntry SelectedEntry => _currentHistoryIndex > -1 && _visibleHistory.Count > 0 ? _visibleHistory[_currentHistoryIndex] : null;
        
        // History
        public List<HistoryEntry> HistoryEntries => SerializedHistory.Instance.history;
        
        private List<HistoryEntry> _visibleHistory = new List<HistoryEntry>();
        public List<HistoryEntry> DisplayedHistoryEntries => _visibleHistory;
        
        // Favourites
        private List<HistoryEntry> _rawFavourites = new List<HistoryEntry>();
        public List<HistoryEntry> FavouriteEntries => _rawFavourites;
        
        public bool CanGoBack => _visibleHistory.Count > 1 && _currentHistoryIndex < _visibleHistory.Count - 1 && _currentHistoryIndex >= 0;
        public bool CanGoForward => _currentHistoryIndex > 0;
        
        public HistoryService()
        {
            LoadHistoryFromAsset();
            Selection.selectionChanged += OnSelectionChanged;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            if (SerializedHistory.Instance.showDebug && HistoryEntries.Count == 0) Debug.LogWarning("No history found after loading!");
        }

        public void Dispose()
        {
            SaveHistoryToAsset();
            AssetDatabase.SaveAssetIfDirty(SerializedHistory.Instance);
            Selection.selectionChanged -= OnSelectionChanged;
            EditorSceneManager.sceneOpened -= OnSceneOpened;
        }
        
        private void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            UpdateVisibleHistory();
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
                    // Instead of removing entries I just move them to the back so we can keep their records
                    var temp = _visibleHistory.GetRange(0, Math.Min(_visibleHistory.Count - 1, _currentHistoryIndex));
                    HistoryEntries.RemoveRange(0, _currentHistoryIndex);
                    HistoryEntries.AddRange(temp);
                }

                var historyEntry = HistoryEntries.FirstOrDefault(x => x.Equals(activeObject)) ?? new HistoryEntry(activeObject);

                historyEntry.Value = activeObject;
                historyEntry.Uses++;
                historyEntry.UpdateMetadata();

                int removed = HistoryEntries.RemoveAll(x => x.Equals(activeObject));
                if (SerializedHistory.Instance.showDebug && removed > 1) Debug.LogWarning($"Removed {removed} entries from history for {activeObject.name}!");
                HistoryEntries.Insert(0, historyEntry);

                UpdateVisibleHistory();

                _currentHistoryIndex = 0;
            }
            else
            {
                _currentHistoryIndex = -1;
            }
            
            if (SerializedHistory.Instance.showDebug && HistoryEntries.Count == 0) Debug.LogWarning("Something wiped the history in OnSelectionChanged!");
            
            EditorUtility.SetDirty(SerializedHistory.Instance);
        }

        private void UpdateVisibleHistory()
        {
            _visibleHistory.Clear();
            for (int i = 0; i < SerializedHistory.Instance.MaxHistoryStored; i++)
            {
                if (i >= HistoryEntries.Count) break;
                
                var entry = HistoryEntries[i];
                
                if (entry.IsUnresolved)
                {
                    if (SceneManager.GetSceneByPath(entry.SceneName).isLoaded)
                        entry.TryGetReference();
                    
                    if (entry.IsUnresolved)
                        continue;
                }

                _visibleHistory.Add(entry);
            }
        }
        
        #region Action Callbacks
        public void GoBack()
        {
            if (!CanGoBack) return;

            _currentHistoryIndex++;
            _navigateFlag = true;

            Selection.SetActiveObjectWithContext(_visibleHistory[_currentHistoryIndex].Value, null);
        }

        public void GoForward()
        {
            if (!CanGoForward) return;

            _currentHistoryIndex--;
            _navigateFlag = true;

            Selection.SetActiveObjectWithContext(_visibleHistory[_currentHistoryIndex].Value, null);
        }
        
        public void SelectHistoryItem(HistoryEntry historyItem)
        {
            int index = _visibleHistory.IndexOf(historyItem);
            if (index == _currentHistoryIndex && Selection.activeObject != null)
                return;

            if (index < SerializedHistory.Instance.maxHistoryDisplayed)
            {
                _currentHistoryIndex = index;
                _navigateFlag = true;
            }
            
            historyItem.Uses++;
            UpdateVisibleHistory();
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

            UpdateVisibleHistory();
        }
        #endregion
        
        #region Save / Load
        public void SaveHistoryToAsset()
        {
            EditorUtility.SetDirty(SerializedHistory.Instance);
            AssetDatabase.SaveAssetIfDirty(SerializedHistory.Instance);
        }

        public void LoadHistoryFromAsset()
        {
            if (SerializedHistory.Instance.showDebug && HistoryEntries.Count == 0) Debug.LogWarning("History is blank before loading!");
            // Resolve ObjectIds into their respective object
            // This is done a bit weirdly like this for performance.
            // It's a slow process so we want to resolve them in bulk.
            var objectIdArray = HistoryEntries
                .Select(x => x.ResolveGlobalId())
                .ToArray();

            Object[] resolvedObjects = new Object[objectIdArray.Length];
            GlobalObjectId.GlobalObjectIdentifiersToObjectsSlow(objectIdArray, resolvedObjects);

            for (int i = 0; i < HistoryEntries.Count; i++)
            {
                HistoryEntries[i].Value = resolvedObjects[i];
            }
                    
            // Initialize favorites list from history items
            _rawFavourites = HistoryEntries.Where(x => x.IsFavourite).ToList();
                    
            // Initialize other lists
            UpdateVisibleHistory();
        }
        
        public void ClearHistory()
        {
            HistoryEntries.Clear();
            _rawFavourites.Clear();
            _visibleHistory.Clear();
            _currentHistoryIndex = 0;
        }
        #endregion
    }
}
