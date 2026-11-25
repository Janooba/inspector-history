using System;
using System.Collections.Generic;
using System.IO;
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
        /// Versions 1.1.2 and earlier stored the history in this asset.
        private const string LEGACY_ASSET_PATH = "Assets/Resources/SelectionHistoryConfig.asset";

        /// The GUID of the removed SerializedHistory script, which the legacy asset references.
        private const string LEGACY_SCRIPT_GUID = "87d94af2e00e4462ea439c46e5ddeb23";

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
        public List<HistoryEntry> HistoryEntries => HistoryData.instance.history;

        private List<HistoryEntry> _visibleHistory = new List<HistoryEntry>();
        public List<HistoryEntry> DisplayedHistoryEntries => _visibleHistory;

        // Favourites
        public List<HistoryEntry> FavouriteEntries => HistoryData.instance.favourites;

        public bool CanGoBack => FindLoadedIndex(_currentHistoryIndex + 1, 1) >= 0;
        public bool CanGoForward => _currentHistoryIndex > 0 && FindLoadedIndex(_currentHistoryIndex - 1, -1) >= 0;

        public HistoryService()
        {
            // Assets do not load reliably during InitializeOnLoad.
            EditorApplication.delayCall += Initialize;
        }

        public void Dispose()
        {
            SaveHistory();
            EditorApplication.delayCall -= Initialize;
            Selection.selectionChanged -= OnSelectionChanged;
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneManagerSetupRestored -= OnSceneManagerRestored;
            AssemblyReloadEvents.beforeAssemblyReload -= SaveHistory;
            EditorApplication.quitting -= SaveHistory;
        }

        private void Initialize()
        {
            DeleteLegacyAsset();
            LoadHistory();
            Selection.selectionChanged += OnSelectionChanged;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.sceneManagerSetupRestored += OnSceneManagerRestored;
            AssemblyReloadEvents.beforeAssemblyReload += SaveHistory;
            EditorApplication.quitting += SaveHistory;
            if (HistoryData.instance.showDebug && HistoryEntries.Count == 0) Debug.LogWarning("No history found after loading!");
        }

        private static void DeleteLegacyAsset()
        {
            if (!File.Exists(LEGACY_ASSET_PATH)) return;

            // The script GUID identifies the asset, because a different asset can have the same path.
            if (!File.ReadAllText(LEGACY_ASSET_PATH).Contains($"guid: {LEGACY_SCRIPT_GUID}")) return;

            if (AssetDatabase.DeleteAsset(LEGACY_ASSET_PATH))
            {
                Debug.Log($"Inspector History: Deleted the legacy history asset at {LEGACY_ASSET_PATH}. History is now saved in UserSettings.");
            }
        }

        private void OnSceneManagerRestored(Scene[] scenes)
        {
            ResolveFavourites();
            UpdateVisibleHistory();
        }

        private void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            ResolveFavourites();
            UpdateVisibleHistory();
        }

        private void ResolveFavourites()
        {
            foreach (var entry in FavouriteEntries)
            {
                if (entry.IsUnresolved) entry.TryResolveReference();
            }
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

            if (activeObject != null && !IsSkippedFolder(activeObject))
            {
                if (_currentHistoryIndex > 0)
                {
                    // If you're in the past, we need to drop the old future for the new future
                    // Instead of removing entries I just move them to the back so we can keep their records
                    var temp = _visibleHistory.GetRange(0, Math.Min(_visibleHistory.Count - 1, _currentHistoryIndex));
                    HistoryEntries.RemoveRange(0, _currentHistoryIndex);
                    HistoryEntries.AddRange(temp);
                }

                string activeId = HistoryEntry.GetObjectGlobalId(activeObject);
                var historyEntry = HistoryEntries.FirstOrDefault(x => IsSameObject(x, activeObject, activeId)) ?? new HistoryEntry(activeObject);

                historyEntry.Value = activeObject;
                historyEntry.Uses++;
                historyEntry.UpdateMetadata();

                int removed = HistoryEntries.RemoveAll(x => IsSameObject(x, activeObject, activeId));
                if (HistoryData.instance.showDebug && removed > 1) Debug.LogWarning($"Removed {removed} entries from history for {activeObject.name}!");
                HistoryEntries.Insert(0, historyEntry);

                HistoryData.instance.Trim();

                UpdateVisibleHistory();

                _currentHistoryIndex = 0;
            }
            else
            {
                _currentHistoryIndex = -1;
            }

            if (HistoryData.instance.showDebug && HistoryEntries.Count == 0) Debug.LogWarning("Something wiped the history in OnSelectionChanged!");

            SaveHistory();
        }

        /// Compares ids only for unloaded entries, because GetObjectGlobalId is slow.
        private static bool IsSameObject(HistoryEntry entry, Object obj, string objId)
        {
            return entry.Value ? entry.Value == obj : entry.GlobalId == objId;
        }

        private static bool IsSkippedFolder(Object obj)
        {
            return HistoryData.instance.skipFolders && AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(obj));
        }

        private void UpdateVisibleHistory()
        {
            _visibleHistory.Clear();
            for (int i = 0; i < HistoryData.instance.MaxHistoryStored; i++)
            {
                if (i >= HistoryEntries.Count) break;

                var entry = HistoryEntries[i];

                if (entry.IsUnresolved && !entry.TryResolveReference() && entry.IsAssetMissing)
                {
                    HistoryEntries.RemoveAt(i);
                    i--;
                    continue;
                }

                _visibleHistory.Add(entry);
            }
        }

        #region Action Callbacks
        public void GoBack()
        {
            int index = FindLoadedIndex(_currentHistoryIndex + 1, 1);
            if (index >= 0) NavigateTo(index);
        }

        public void GoForward()
        {
            int index = FindLoadedIndex(_currentHistoryIndex - 1, -1);
            if (index >= 0) NavigateTo(index);
        }

        public void SelectHistoryItem(HistoryEntry historyItem)
        {
            if (historyItem.Value == Selection.activeObject) return;

            historyItem.Uses++;
            int index = _visibleHistory.IndexOf(historyItem);
            if (index >= 0 && index < HistoryData.instance.maxHistoryDisplayed)
            {
                NavigateTo(index);
            }
            else
            {
                // Entries outside the displayed history are recorded as a new selection.
                Selection.SetActiveObjectWithContext(historyItem.Value, null);
            }
        }

        private void NavigateTo(int index)
        {
            _currentHistoryIndex = index;
            var target = _visibleHistory[index].Value;

            // Selection.selectionChanged does not fire when the selection stays the same.
            _navigateFlag = target != Selection.activeObject;
            Selection.SetActiveObjectWithContext(target, null);
        }

        /// <returns> The first index from start, in the direction of step, with a loaded object, or -1. </returns>
        private int FindLoadedIndex(int start, int step)
        {
            for (int i = start; i >= 0 && i < _visibleHistory.Count; i += step)
            {
                if (!_visibleHistory[i].IsUnresolved) return i;
            }

            return -1;
        }

        public bool IsFavourite(HistoryEntry historyItem)
        {
            return FavouriteEntries.Contains(historyItem);
        }

        public void ToggleFavourite(HistoryEntry historyItem)
        {
            if (!FavouriteEntries.Remove(historyItem))
            {
                FavouriteEntries.Add(historyItem);
            }

            SaveHistory();
        }
        #endregion

        #region Save / Load
        public void SaveHistory()
        {
            HistoryData.instance.SaveToFile();
        }

        public void LoadHistory()
        {
            if (HistoryData.instance.showDebug && HistoryEntries.Count == 0)
                Debug.LogWarning("History is blank before loading!");

            ResolveInBulk(HistoryEntries);
            ResolveInBulk(FavouriteEntries);

            UpdateVisibleHistory();
        }

        private static void ResolveInBulk(List<HistoryEntry> entries)
        {
            // Resolve ObjectIds into their respective object
            // This is done a bit weirdly like this for performance.
            // It's a slow process so we want to resolve them in bulk.
            var objectIdArray = entries
                .Select(x => x.ResolveGlobalId())
                .ToArray();

            Object[] resolvedObjects = new Object[objectIdArray.Length];
            GlobalObjectId.GlobalObjectIdentifiersToObjectsSlow(objectIdArray, resolvedObjects);

            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].Value = resolvedObjects[i];
            }
        }

        public void ClearHistory()
        {
            HistoryEntries.Clear();
            FavouriteEntries.Clear();
            _visibleHistory.Clear();
            _currentHistoryIndex = 0;
        }
        #endregion
    }
}
