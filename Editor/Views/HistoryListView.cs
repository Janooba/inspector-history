using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VoidState.InspectorHistory.Editor
{
    public class HistoryListView
    {
        private string _title;
        private readonly int _maxVisible;
        private readonly bool _reverseOrder;

        public HistoryListView(string title, int maxVisible = int.MaxValue, bool reverseOrder = false)
        {
            _title = title;
            _maxVisible = maxVisible;
            _reverseOrder = reverseOrder;
        }

        public void Draw(List<HistoryEntry> history, IHistoryInteraction interaction)
        {
            Draw_Internal(history, interaction, null);
        }

        public void Draw(List<HistoryEntry> history, HistoryEntry selectedEntry, IHistoryInteraction interaction)
        {
            if (selectedEntry == null)
                Draw_Internal(history, interaction, null);
            else
                Draw_Internal(history, interaction, (entry, index) => entry.Equals(selectedEntry));
        }

        public void Draw(List<HistoryEntry> history, int selectedIndex, IHistoryInteraction interaction)
        {
            Draw_Internal(history, interaction, (entry, index) => index == selectedIndex);
        }

        private delegate bool TestSelection(HistoryEntry entry, int index);
        private void Draw_Internal(List<HistoryEntry> history, IHistoryInteraction interaction, TestSelection selectionTest = null)
        {
            EditorGUILayout.BeginVertical();

            if (!string.IsNullOrEmpty(_title)) Utilities.DrawTitle(_title);

            if (history.Count > 0)
            {
                if (_reverseOrder)
                {
                    for (int i = Math.Min(history.Count - 1, _maxVisible); i >= 0; i--)
                        history[i].Draw(selectionTest?.Invoke(history[i], i) ?? false, 
                            (entry) => interaction.SelectHistoryItem(entry),
                            (entry) => interaction.ToggleFavourite(entry));
                }
                else
                {
                    for (int i = 0; i < Math.Min(history.Count, _maxVisible); i++)
                        history[i].Draw(selectionTest?.Invoke(history[i], i) ?? false, 
                            (entry) => interaction.SelectHistoryItem(entry),
                            (entry) => interaction.ToggleFavourite(entry));
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
