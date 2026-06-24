using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VoidState.InspectorHistory.Editor
{
    public class InspectorHistoryWindow : EditorWindow
    {
        [Serializable]
        private struct HistoryEntry : IEquatable<HistoryEntry>
        {
            public Object Value;
            public string Path;


            public bool Equals(HistoryEntry other)
            {
                return Equals(Value, other.Value);
            }

            public override bool Equals(object obj)
            {
                return obj is HistoryEntry other && Equals(other);
            }

            public override int GetHashCode()
            {
                return (Value != null ? Value.GetHashCode() : 0);
            }
        }
        
        private const int NAV_BTN_HEIGHT = 40;
        private const int HISTORY_MAX = 10;
        
        [MenuItem("VoidState/Inspector History")]
        public static void OpenWindow()
        {
            var wnd = GetWindow<InspectorHistoryWindow>();
            wnd.titleContent = new GUIContent("Inspector History");
        }

        private List<HistoryEntry> _rawHistory = new List<HistoryEntry>();

        /// True if navigation was triggered by this toolset and not a user selecting something.
        /// Used to refrain from updating the history while navigating.
        private bool _navigateFlag = false;

        /// Where we are in the current history. 0 index is most recent, 1 is last, 2 is one before last, etc.
        private int _currentHistoryIndex = 0;

        private GUIContent _iconBack;
        private GUIContent _iconForward;
        private GUIContent _iconSelect;
        private GUIContent _iconStar;
        private Texture2D _highlightTex;
        private GUIStyle _highlightRowStyle;
        private GUIStyle _rowStyle;
        private bool _stylesReady;
        
        protected void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;
            
            _iconBack = EditorGUIUtility.IconContent("back@2x");
            _iconForward = EditorGUIUtility.IconContent("forward@2x");
            _iconSelect = EditorGUIUtility.IconContent("scenepicking_pickable@2x");
            _iconStar = EditorGUIUtility.IconContent("Favorite");

            _highlightTex = MakeTex(2, 2, new Color(0f, 1f, 1f, 0.3f));
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
                if (_currentHistoryIndex > 0)
                {
                    // If you're in the past, we need to drop the old future for the new future
                    // ...what?
                    _rawHistory.RemoveRange(0, _currentHistoryIndex);
                }
                
                _rawHistory.RemoveAll(x => x.Value == activeObject);

                _rawHistory.Insert(0, new HistoryEntry { Value = activeObject });
                
                if (_rawHistory.Count > HISTORY_MAX)
                    _rawHistory.RemoveAt(_rawHistory.Count - 1);
                
                _currentHistoryIndex = 0;
            }
            else
            {
                _currentHistoryIndex = -1;
            }
            Repaint();
        }

        public void GoBack()
        {
            if (ShouldDisableBackButton()) return;

            _currentHistoryIndex++;
            _navigateFlag = true;

            Selection.SetActiveObjectWithContext(_rawHistory[_currentHistoryIndex].Value, null);
        }

        private bool ShouldDisableBackButton()
        {
            return _rawHistory.Count <= 1 || _currentHistoryIndex == _rawHistory.Count - 1 || _currentHistoryIndex < 0;
        }

        public void GoForward()
        {
            if (ShouldDisableForwardButton()) return;

            _currentHistoryIndex--;
            _navigateFlag = true;

            Selection.SetActiveObjectWithContext(_rawHistory[_currentHistoryIndex].Value, null);
        }

        private bool ShouldDisableForwardButton()
        {
            return _currentHistoryIndex <= 0;
        }
        
        private void OnGUI()
        {
            // Init styles
            if (!_stylesReady)
            {
                _rowStyle = new GUIStyle(GUI.skin.box);
                _highlightRowStyle = new GUIStyle(_rowStyle);
                _highlightRowStyle.normal.background = _highlightTex;
                _stylesReady = true;
            }
            
            // Back and Forward buttons
            GUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(ShouldDisableBackButton());
            if (GUILayout.Button(_iconBack, GUILayout.Height(NAV_BTN_HEIGHT)))
            {
                GoBack();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(ShouldDisableForwardButton());
            if (GUILayout.Button(_iconForward, GUILayout.Height(NAV_BTN_HEIGHT)))
            {
                GoForward();
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();

            // Add some spacing
            GUILayout.Space(10);

            // History list
            if (_rawHistory.Count > 0)
            {
                //for (int i = _rawHistory.Count - 1; i >= 0; i--)
                for (int i = 0; i < _rawHistory.Count; i++)
                {
                    var historyItem = _rawHistory[i];
                    if (historyItem.Value == null) continue;
                    
                    GUILayout.BeginHorizontal(_currentHistoryIndex == i ? _highlightRowStyle : _rowStyle);
                    
                    // Star / Fav
                    if (GUILayout.Button(_iconStar, EditorStyles.iconButton))
                    {
                        
                    }
                    
                    // Icon and label
                    var icon = AssetPreview.GetMiniThumbnail(historyItem.Value);
                    GUILayout.Label(icon, EditorStyles.iconButton, GUILayout.Width(18));
                    GUILayout.Label(historyItem.Value.name);

                    // Select button
                    if (GUILayout.Button(_iconSelect, EditorStyles.iconButton))
                    {
                        _currentHistoryIndex = i;
                        _navigateFlag = true;
                        Selection.SetActiveObjectWithContext(historyItem.Value, null);
                    }

                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                GUILayout.Label("No history available");
            }
        }
        
        private Texture2D MakeTex( int width, int height, Color col )
        {
            Color[] pix = new Color[width * height];
            for( int i = 0; i < pix.Length; ++i )
            {
                pix[ i ] = col;
            }
            Texture2D result = new Texture2D( width, height );
            result.SetPixels( pix );
            result.Apply();
            return result;
        }
    }
}