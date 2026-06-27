using System;
using UnityEditor;

namespace VoidState.InspectorHistory.Editor
{
    public interface IHistoryInteraction
    {
        void SelectHistoryItem(HistoryEntry historyItem);
        void ToggleFavourite(HistoryEntry historyItem);
    }
}