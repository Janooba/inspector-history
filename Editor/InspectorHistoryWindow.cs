using System;
using UnityEditor;
using UnityEngine;

namespace VoidState.InspectorHistory.Editor
{
    // Useful link for icons https://github.com/halak/unity-editor-icons
    public class InspectorHistoryWindow : EditorWindow
    {
        public const int HISTORY_MAX = 10;
        public const int FREQUENT_MAX = 5;

        [MenuItem("Tools/VoidState/Inspector History")]
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
            _frequentView ??= new EntryListView(_history, "Frequent", FREQUENT_MAX);
            _favoriteView ??= new EntryListView(_history, "Favourites");
            _entryView ??= new EntryListView(_history, "History", HISTORY_MAX);
        }

        private void OnGUI()
        {
            InitializeViews();

            using (var scrollView = new EditorGUILayout.ScrollViewScope(_scrollPosition, GUILayout.ExpandWidth(false)))
            {
                _scrollPosition = scrollView.scrollPosition;
                
                _favoriteView.Draw(_history.FavouriteEntries, true, false);

                if (_history.FrequentEntries.Count > 0)
                {
                    _frequentView.Draw(_history.FrequentEntries, true, false);
                }

                _entryView.Draw(_history.DisplayedHistoryEntries, true, false);
            }
        }
    }
}