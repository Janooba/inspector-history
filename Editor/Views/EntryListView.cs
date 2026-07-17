using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VoidState.InspectorHistory.Editor
{
    public class EntryListView
    {
        private HistoryService _service;
        private string _title;
        private readonly int _maxVisible;
        public int MaxVisible => _maxVisible;
        private readonly bool _reverseOrder;

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    EditorPrefs.SetBool($"{Utilities.PREFS_PREFIX}.{_title}Expanded", value);
                }
            }
        }

        public EntryListView(HistoryService service, string title, int maxVisible = int.MaxValue, bool reverseOrder = false)
        {
            _service = service;
            _title = title;
            _maxVisible = maxVisible;
            _reverseOrder = reverseOrder;
            _isExpanded = EditorPrefs.GetBool($"{Utilities.PREFS_PREFIX}.{_title}Expanded", true);
        }

        public void Draw(List<HistoryEntry> history, bool showSelected, bool showMissing, bool showSceneSeparately)
        {
            EditorGUILayout.BeginVertical();

            if (!string.IsNullOrEmpty(_title))
            {
                bool isExpanded = IsExpanded;
                Utilities.DrawTitleFoldout(_title, ref isExpanded);
                IsExpanded = isExpanded;
            }

            List<HistoryEntry> projectEntries = new List<HistoryEntry>();
            Dictionary<string, List<HistoryEntry>> sceneLocations = new Dictionary<string, List<HistoryEntry>>();
            
            if (IsExpanded)
            {
                if (showSceneSeparately)
                {
                    // Categorize entries
                    for (int i = history.Count - 1; i >= 0; i--)
                    {
                        var entry = history[i];
                        if (entry.IsUnresolved && !showMissing) continue;
                    
                        if (entry.IsPersistentAsset)
                        {
                            projectEntries.Add(entry);
                        }
                        else
                        {
                            if (sceneLocations.ContainsKey(entry.NiceSceneName))
                            {
                                sceneLocations[entry.NiceSceneName].Add(entry);
                            }
                            else
                            {
                                sceneLocations.Add(entry.NiceSceneName, new List<HistoryEntry> { entry });
                            }
                        }
                    }
                    
                    // Project
                    DrawList(projectEntries, showSelected, showMissing, _reverseOrder);

                    // Scene locations
                    foreach (var (name, list) in sceneLocations)
                    {
                        Utilities.DrawSubHeader(name);
                        DrawList(list, showSelected, showMissing, _reverseOrder);
                    }
                }
                else
                {
                    DrawList(history, showSelected, showMissing, _reverseOrder);
                }
            }
            
            EditorGUILayout.LabelField("", GUILayout.Height(1));
            EditorGUILayout.EndVertical();
        }

        private int DrawList(List<HistoryEntry> history, bool showSelected, bool showMissing, bool reverseOrder)
        {
            int displayed = 0;

            if (reverseOrder)
            {
                for (int i = history.Count - 1; i >= 0; i--)
                {
                    var entry = history[i];
                    if (displayed > _maxVisible) break;
                    if (entry.IsUnresolved && !showMissing) continue;
                        
                    entry.Draw(showSelected && entry.Equals(_service.SelectedEntry),
                        _service.SelectHistoryItem, _service.ToggleFavourite);
                    displayed++;
                }
            }
            else
            {
                for (int i = 0; i < Math.Min(history.Count, _maxVisible); i++)
                {
                    if (displayed > _maxVisible) break;
                    if (history[i].IsUnresolved && !showMissing) continue;
                        
                    history[i].Draw(showSelected && history[i].Equals(_service.SelectedEntry),
                        _service.SelectHistoryItem, _service.ToggleFavourite);
                    displayed++;
                }
            }
            
            if (displayed == 0)
            {
                GUILayout.Label("No history available");
            }

            return displayed;
        }
    }
}
