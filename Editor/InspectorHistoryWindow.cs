using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VoidState.InspectorHistory.Editor
{
    public class InspectorHistoryWindow : EditorWindow
    {
        [MenuItem("VoidState/Inspector History")]
        public static void OpenWindow()
        {
            var wnd = GetWindow<InspectorHistoryWindow>();
            wnd.titleContent = new GUIContent("Inspector History");
        }

        private List<Object> _rawHistory = new List<Object>();
        
        /// True if navigation was triggered by this toolset and not a user selecting something.
        /// Used to refrain from updating the history while navigating.
        private bool _navigateFlag = false;
        
        /// Where we are in the current history. 0 index is most recent, 1 is last, 2 is one before last, etc.
        private int _currentHistoryIndex = 0;
        
        protected void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;
        }

        protected void OnDisable()
        {
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
                if (_rawHistory.Contains(activeObject))
                    _rawHistory.Remove(activeObject);
                
                _rawHistory.Insert(0, activeObject);
            }
        }
        
        public void GoBack()
        {
            if (ShouldDisableBackButton()) return;
            
            _currentHistoryIndex++;
            _navigateFlag = true;
            
            Selection.SetActiveObjectWithContext(_rawHistory[_currentHistoryIndex], null);
        }

        private bool ShouldDisableBackButton()
        {
            return _rawHistory.Count <= 1 || _currentHistoryIndex == _rawHistory.Count - 1;
        }

        public void GoForward()
        {
            if (ShouldDisableForwardButton()) return;
            
            _currentHistoryIndex--;
            _navigateFlag = true;
            
            Selection.SetActiveObjectWithContext(_rawHistory[_currentHistoryIndex], null);
        }
        
        private bool ShouldDisableForwardButton()
        {
            return _currentHistoryIndex == 0;
        }

        private void OnGUI()
        {
            
        }
    }
}