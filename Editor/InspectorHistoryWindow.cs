using System;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace VoidState.InspectorHistory.Editor
{
    // Useful link for icons https://github.com/halak/unity-editor-icons
    public class InspectorHistoryWindow : EditorWindow
    {
        [MenuItem("Tools/VoidState/Inspector History")]
        public static void OpenWindow()
        {
            var wnd = GetWindow<InspectorHistoryWindow>();
            wnd.titleContent = new GUIContent("Inspector History");
        }

        [Shortcut("history_back", KeyCode.Mouse3, ShortcutModifiers.None, displayName = "Selection History - Back")]
        public static void GoBack()
        {
            HistoryService.Instance.GoBack();
        }
        
        [Shortcut("history_forward", KeyCode.Mouse4, ShortcutModifiers.None, displayName = "Selection History - Forward")]
        public static void GoForward()
        {
            HistoryService.Instance.GoForward();
        }

        private HistoryService _history;
        private NavbarView _navbarView;
        private EntryListView _favoriteView;
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
            _favoriteView ??= new EntryListView(_history, "Favourites");
            if (_entryView == null || _entryView.MaxVisible != SerializedHistory.Instance.maxHistoryDisplayed) 
                _entryView = new EntryListView(_history, "History", SerializedHistory.Instance.maxHistoryDisplayed);
        }

        private void OnGUI()
        {
            InitializeViews();

            _navbarView.Draw();
            
            using (var scrollView = new EditorGUILayout.ScrollViewScope(_scrollPosition, GUILayout.ExpandWidth(false)))
            {
                _scrollPosition = scrollView.scrollPosition;
                
                _favoriteView.Draw(_history.FavouriteEntries, true, false, SerializedHistory.Instance.showSceneObjectsSeparately);

                _entryView.Draw(_history.DisplayedHistoryEntries, true, false, false);
            }
        }
    }
}