using System;
using UnityEditor;
using UnityEngine;

namespace VoidState.InspectorHistory.Editor
{
    // Useful link for icons https://github.com/halak/unity-editor-icons
    public class InspectorHistoryWindow : EditorWindow
    {
        private const int HISTORY_MAX = 20;

        [MenuItem("VoidState/Inspector History")]
        public static void OpenWindow()
        {
            var wnd = GetWindow<InspectorHistoryWindow>();
            wnd.titleContent = new GUIContent("Inspector History");
        }

        private HistoryService _history;
        private NavbarView _navbarView;
        private EntryListView _favoriteView;
        private EntryListView _frequentView;
        private EntryListView _entryView;

        private Vector2 _scrollPosition;

        protected void OnEnable()
        {
            _history = HistoryService.Instance;
            InitializeViews();
            Selection.selectionChanged += Repaint;
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= Repaint;
        }

        private void InitializeViews()
        {
            _navbarView ??= new NavbarView(_history);
            _frequentView ??= new EntryListView(_history, "Frequent");
            _favoriteView ??= new EntryListView(_history, "Favourites");
            _entryView ??= new EntryListView(_history, "History", HISTORY_MAX / 2);
        }

        private void OnGUI()
        {
            InitializeViews();
            
            _navbarView.Draw();

            using (var scrollView = new EditorGUILayout.ScrollViewScope(_scrollPosition, GUILayout.ExpandWidth(false)))
            {
                _scrollPosition = scrollView.scrollPosition;
                
                if (_history.FavouriteEntries.Count > 0)
                {
                    _favoriteView.Draw(_history.FavouriteEntries, true);
                }

                if (_history.FrequentEntries.Count > 0)
                {
                    _frequentView.Draw(_history.FrequentEntries, true);
                }

                _entryView.Draw(_history.HistoryEntries, true);
            }
            
            Utilities.DrawSeparator();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear")) _history.ClearHistory();
            if (GUILayout.Button("Save")) _history.SaveHistoryToEditorPrefs();
            if (GUILayout.Button("Load")) _history.LoadHistoryFromEditorPrefs();
            EditorGUILayout.EndHorizontal();
        }
    }
}