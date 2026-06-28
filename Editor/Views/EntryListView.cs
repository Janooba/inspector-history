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

        public void Draw(List<HistoryEntry> history, bool showSelected, bool showMissing)
        {
            EditorGUILayout.BeginVertical();

            if (!string.IsNullOrEmpty(_title))
            {
                bool isExpanded = IsExpanded;
                Utilities.DrawTitleFoldout(_title, ref isExpanded);
                IsExpanded = isExpanded;
            }

            if (!IsExpanded)
            {
                EditorGUILayout.LabelField("", GUILayout.Height(1));
                EditorGUILayout.EndVertical();
                return;
            }
            
            if (history.Count > 0)
            {
                int displayed = 0;
                if (_reverseOrder)
                {
                    for (int i = history.Count - 1; i >= 0; i--)
                    {
                        if (displayed > _maxVisible) break;
                        if (history[i].IsUnresolved && !showMissing) continue;
                        
                        history[i].Draw(showSelected && history[i].Equals(_service.SelectedEntry),
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
            }
            else
            {
                GUILayout.Label("No history available");
            }

            EditorGUILayout.EndVertical();
        }
    }
}
