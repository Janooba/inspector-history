using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Sirenix.Serialization;
using Object = UnityEngine.Object;
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
        
        /// True if navigation was triggered by this toolset and not a user selecting something.
        /// Used to refrain from updating the history while navigating.
        private bool _navigateFlag = false;

        /// Where we are in the current history. 0 index is most recent, 1 is last, 2 is one before last, etc.
        private int _currentHistoryIndex = 0;
        
        public HistoryEntry SelectedEntry => _currentHistoryIndex > -1 && _visibleHistory.Count > 0 ? _visibleHistory[_currentHistoryIndex] : null;
        
        // History
        private List<HistoryEntry> _rawHistory = new List<HistoryEntry>();
        public List<HistoryEntry> HistoryEntries => _rawHistory;
        
        private List<HistoryEntry> _visibleHistory = new List<HistoryEntry>();
        public List<HistoryEntry> DisplayedHistoryEntries => _visibleHistory;
        
        // Favourites
        private List<HistoryEntry> _rawFavourites = new List<HistoryEntry>();
        public List<HistoryEntry> FavouriteEntries => _rawFavourites;
        
        // Frequents
        private List<HistoryEntry> _rawFrequent = new List<HistoryEntry>();
        public List<HistoryEntry> FrequentEntries => _rawFrequent;
        
        public bool CanGoBack => _visibleHistory.Count > 1 && _currentHistoryIndex < _visibleHistory.Count - 1 && _currentHistoryIndex >= 0;
        public bool CanGoForward => _currentHistoryIndex > 0;
        
        public HistoryService()
        {
            LoadHistoryFromAsset();
            Selection.selectionChanged += OnSelectionChanged;
        }
        
        public void Dispose()
        {
            SaveHistoryToAsset();
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
                    // Instead of removing entries I just move them to the back so we can keep their records
                    var temp = _visibleHistory.GetRange(0, _currentHistoryIndex);
                    _rawHistory.RemoveRange(0, _currentHistoryIndex);
                    _rawHistory.AddRange(temp);
                }

                var historyEntry = _rawHistory.FirstOrDefault(x => x.Value == activeObject) ?? new HistoryEntry(activeObject);

                historyEntry.Uses++;

                _rawHistory.RemoveAll(x => x.Value == activeObject);
                _rawHistory.Insert(0, historyEntry);

                UpdateFrequent();
                UpdateVisibleHistory();

                _currentHistoryIndex = 0;
            }
            else
            {
                _currentHistoryIndex = -1;
            }

            // Save history to EditorPrefs
            SaveHistoryToAsset();
        }
        
        private void UpdateFrequent()
        {
            _rawFrequent = _rawHistory.OrderByDescending(x => x.Uses)
                .Where(x => !x.IsFavourite)
                .ToList();
        }

        private void UpdateVisibleHistory()
        {
            _visibleHistory = _rawHistory
                .Where(x => !x.IsUnresolved)
                .ToList();
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
            if (index == _currentHistoryIndex) return;

            if (index < InspectorHistoryWindow.HISTORY_MAX)
            {
                _currentHistoryIndex = index;
                _navigateFlag = true;
            }
            
            historyItem.Uses++;
            UpdateFrequent();
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

            UpdateFrequent();
            UpdateVisibleHistory();
        }
        #endregion
        
        #region Save / Load
        public void SaveHistoryToAsset()
        {
            SerializedHistory.Instance.history = _rawHistory;
            EditorUtility.SetDirty(SerializedHistory.Instance);
            AssetDatabase.SaveAssetIfDirty(SerializedHistory.Instance);
        }

        public void LoadHistoryFromAsset()
        {
            _rawHistory = SerializedHistory.Instance.history;
                    
            // Resolve ObjectIds into their respective object
            // This is done a bit weirdly like this for performance.
            // It's a slow process so we want to resolve them in bulk.
            var objectIdArray = _rawHistory
                .Select(x => x.ResolveGlobalId())
                .ToArray();

            Object[] resolvedObjects = new Object[objectIdArray.Length];
            GlobalObjectId.GlobalObjectIdentifiersToObjectsSlow(objectIdArray, resolvedObjects);

            for (int i = 0; i < _rawHistory.Count; i++)
            {
                _rawHistory[i].Value = resolvedObjects[i];
            }
                    
            // Initialize favorites list from history items
            _rawFavourites = _rawHistory.Where(x => x.IsFavourite).ToList();
                    
            // Initialize other lists
            UpdateFrequent();
            UpdateVisibleHistory();
        }
        
        public void ClearHistory()
        {
            _rawFavourites.Clear();
            _rawHistory.Clear();
            _rawFrequent.Clear();
            _visibleHistory.Clear();
            _currentHistoryIndex = 0;
        }
        #endregion
    }
}
