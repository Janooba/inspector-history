using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VoidState.InspectorHistory.Editor
{
    // Useful link for icons https://github.com/halak/unity-editor-icons
    public class InspectorHistoryWindow : EditorWindow
    {
        [Serializable]
        private class HistoryEntry : IEquatable<HistoryEntry>, IComparable<HistoryEntry>
        {
            public Object Value;
            
            public string Path = "";
            public int Uses;
            public bool IsFavourite;
            public bool IsPersistentAsset;
            public string SceneName = "";

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

            public int CompareTo(HistoryEntry other)
            {
                return Uses.CompareTo(other.Uses);
            }
        }

        private const string PACKAGE_PATH = "Packages/com.voidstate.inspector-history";
        
        private const int NAV_BTN_HEIGHT = 40;
        
        private const int HISTORY_VISIBLE = 10;
        private const int HISTORY_MAX = HISTORY_VISIBLE * 2;
        
        [MenuItem("VoidState/Inspector History")]
        public static void OpenWindow()
        {
            var wnd = GetWindow<InspectorHistoryWindow>();
            wnd.titleContent = new GUIContent("Inspector History");
        }
        
        private List<HistoryEntry> _rawFavorites = new List<HistoryEntry>();
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
        private GUIContent _iconStarSelected;
        private Texture2D _highlightTex;
        private GUIStyle _highlightRowStyle;
        private GUIStyle _rowStyle;
        private GUIStyle _rowButton;
        private GUIStyle _rowButtonHighlight;
        private bool _stylesReady;
        
        protected void OnEnable()
        {
            _stylesReady = false;
            Selection.selectionChanged += OnSelectionChanged;
            
            _iconBack = EditorGUIUtility.IconContent("back@2x");
            _iconForward = EditorGUIUtility.IconContent("forward@2x");
            _iconSelect = EditorGUIUtility.IconContent("scenepicking_pickable@2x");
            Texture favoriteIcon = AssetDatabase.LoadAssetAtPath<Texture>($"{PACKAGE_PATH}/Editor/Images/icon_favorite.png");
            _iconStar = new GUIContent(favoriteIcon);
            Texture favoriteIconOn = AssetDatabase.LoadAssetAtPath<Texture>($"{PACKAGE_PATH}/Editor/Images/icon_favorite_on.png");
            _iconStarSelected = new GUIContent(favoriteIconOn);
            
            _highlightTex = MakeTex(2, 2, new Color(0f, 1f, 1f, 0.2f));
        }

        protected void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private void InitializeStyles()
        {
            _rowStyle = new GUIStyle(GUI.skin.box);
            _highlightRowStyle = new GUIStyle(_rowStyle);
            _highlightRowStyle.normal.background = _highlightTex;

            _rowButton = new GUIStyle(EditorStyles.iconButton);
            _rowButtonHighlight = new GUIStyle(_rowButton);
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
                    _rawHistory.RemoveRange(0, _currentHistoryIndex);
                }
                
                var historyEntry = _rawHistory.FirstOrDefault(x => x.Value == activeObject) ?? new HistoryEntry
                {
                    Value = activeObject,
                    Path = activeObject.name,
                    Uses = 0,
                    IsFavourite = false,
                    IsPersistentAsset = EditorUtility.IsPersistent(activeObject),
                };

                if (!historyEntry.IsPersistentAsset && activeObject is GameObject activeGameObject)
                {
                    historyEntry.SceneName = activeGameObject.scene.path;
                    historyEntry.Path = SearchUtils.GetTransformPath(activeGameObject.transform);
                }
                else
                {
                    historyEntry.SceneName = "";
                    historyEntry.Path = SearchUtils.GetObjectPath(activeObject);
                }
                
                historyEntry.Uses++;
                
                _rawHistory.RemoveAll(x => x.Value == activeObject);
                _rawHistory.Insert(0, historyEntry);
                
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
            InitializeStyles();
                
            DrawNavToolbar();

            // Add some spacing
            DrawSeparator();
            
            // Favorite List
            if (_rawFavorites.Count > 0)
            {
                EditorGUILayout.LabelField("Favourites");
                for (int i = 0; i < _rawFavorites.Count; i++)
                {
                    var historyItem = _rawFavorites[i];
                    DrawHistoryEntry(historyItem, Selection.activeObject == historyItem.Value);
                }
                DrawSeparator();
            }
            
            EditorGUILayout.LabelField("History");
            // History list
            if (_rawHistory.Count > 0)
            {
                //for (int i = _rawHistory.Count - 1; i >= 0; i--)
                for (int i = 0; i < _rawHistory.Count && i < HISTORY_VISIBLE; i++)
                {
                    var historyItem = _rawHistory[i];
                    DrawHistoryEntry(historyItem, _currentHistoryIndex == i);
                }
            }
            else
            {
                GUILayout.Label("No history available");
            }
        }

        private void DrawNavToolbar()
        {
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
        }

        private void DrawHistoryEntry(HistoryEntry historyItem, bool isSelected)
        {
            if (historyItem.Value == null) return;
                    
            GUILayout.BeginHorizontal(isSelected ? _highlightRowStyle : _rowStyle);
                    
            // Star / Fav
            if (GUILayout.Button(
                    historyItem.IsFavourite ? _iconStarSelected : _iconStar, 
                    historyItem.IsFavourite ? _rowButton : _rowButtonHighlight))
            {
                ToggleFavourite(historyItem);
            }
            
            GUILayout.Space(6);
            
            // Icon and label
            var icon = AssetPreview.GetMiniThumbnail(historyItem.Value);
            //GUILayout.Label(icon, EditorStyles.iconButton, GUILayout.Width(18));
            var iconRect = EditorGUILayout.GetControlRect(false, GUILayout.Width(16));
            GUI.DrawTexture(iconRect, icon);
            //EditorGUI.DrawRect(iconRect, new Color(1, 0.5f, 1, 0.3f));
            GUILayout.Label(historyItem.Value.name);

            // // Select button
            // if (GUILayout.Button(_iconSelect, EditorStyles.iconButton))
            // {
            //     SelectHistoryItem(historyItem);
            // }

            GUILayout.EndHorizontal();

            int selectablePadding = 26;
            var entryRect = GUILayoutUtility.GetLastRect();
            entryRect.x += selectablePadding;
            entryRect.width -= selectablePadding;

            //EditorGUI.DrawRect(entryRect, new Color(1, 0.5f, 1, 0.3f));

            if (GUI.Button(entryRect, new GUIContent("", $"{historyItem.Path}\n{historyItem.Value.GetType()}"),
                    GUIStyle.none))
            {
                SelectHistoryItem(historyItem);
            }
        }

        private void SelectHistoryItem(HistoryEntry historyItem)
        {
            int index = _rawHistory.IndexOf(historyItem);
            _currentHistoryIndex = index;
            _navigateFlag = true;
            Selection.SetActiveObjectWithContext(historyItem.Value, null);
        }

        private void ToggleFavourite(HistoryEntry historyItem)
        {
            if (_rawFavorites.Contains(historyItem))
            {
                historyItem.IsFavourite = false;
                _rawFavorites.Remove(historyItem);
            }
            else
            {
                historyItem.IsFavourite = true;
                _rawFavorites.Add(historyItem);
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

        private void DrawSeparator()
        {
            GUILayout.Space(4);
            var spaceRect = EditorGUILayout.GetControlRect(false, 2);
            EditorGUI.DrawRect(spaceRect, Color.black.ChangeAlpha(0.2f));
            GUILayout.Space(4);
        }
    }

    public static class Extensions
    {
        public static Color ChangeAlpha(this Color color, float newAlpha)
        {
            return new Color(color.r, color.g, color.b, newAlpha);
        }
    }
}