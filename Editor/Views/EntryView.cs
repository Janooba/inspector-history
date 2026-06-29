using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VoidState.InspectorHistory.Editor
{
    public static class EntryView
    {
        private const int ROW_HEIGHT = 22;
        
        private static Color _missingColor;
        private static Color _selectedColor;
        
        private static GUIContent _iconStar;
        private static GUIContent _iconStarSelected;
        private static GUIContent _iconWarning;
        
        private static GUIStyle _rowStyle;
        private static GUIStyle _rowButton;
        private static GUIStyle _rowButtonHighlight;
        private static GUIStyle _rowCountStyle;

        static EntryView()
        {
            _missingColor = new Color(1f, 0f, 0f, 0.2f);
            _selectedColor = new Color(0f, 1f, 1f, 0.2f);
        }
        
        private static void EnsureStylesInitialized()
        {
            _rowStyle ??= new GUIStyle();
            _rowStyle.fixedHeight = ROW_HEIGHT;

            _rowButton ??= new GUIStyle(EditorStyles.iconButton);
            _rowButton.fixedHeight = ROW_HEIGHT;
            _rowButton.alignment = TextAnchor.MiddleCenter;
            _rowButtonHighlight ??= new GUIStyle(_rowButton);

            _rowCountStyle ??= new GUIStyle(EditorStyles.miniLabel);
            _rowCountStyle.alignment = TextAnchor.MiddleRight;

            if (_iconStar == null)
            {
                Texture favoriteIcon = AssetDatabase.LoadAssetAtPath<Texture>($"{Utilities.PACKAGE_PATH}/Editor/Images/icon_favorite.png");
                _iconStar = new GUIContent(favoriteIcon);
            }

            if (_iconStarSelected == null)
            {
                Texture favoriteIconOn = AssetDatabase.LoadAssetAtPath<Texture>($"{Utilities.PACKAGE_PATH}/Editor/Images/icon_favorite_on.png");
                _iconStarSelected = new GUIContent(favoriteIconOn);
            }
        }

        public static void Draw(this HistoryEntry entry, bool isSelected, Action<HistoryEntry> selectedCallback, Action<HistoryEntry> favouriteCallback)
        {
            EnsureStylesInitialized();
            
            // Bit of a hack to preemptively get the whole row's width before we lay it out
            GUILayout.Box(GUIContent.none, GUIStyle.none, GUILayout.Height(0));
            var rowRect = GUILayoutUtility.GetLastRect();
            rowRect.height = ROW_HEIGHT;

            if (entry.Value == null)
            {
                EditorGUI.DrawRect(rowRect, _missingColor);
            }
            else if (isSelected)
            {
                EditorGUI.DrawRect(rowRect, _selectedColor);
            }
            
            GUILayout.BeginHorizontal(_rowStyle);

            // Star / Fav
            if (GUILayout.Button(
                    entry.IsFavourite ? _iconStarSelected : _iconStar,
                    entry.IsFavourite ? _rowButton : _rowButtonHighlight))
            {
                favouriteCallback?.Invoke(entry);
            }

            Utilities.DrawVerticalSeparator();

            // Location icon
            var locContent = entry.IsPersistentAsset
                ? EditorGUIUtility.IconContent("Folder Icon")
                : EditorGUIUtility.IconContent("SceneAsset Icon");
            
            locContent.tooltip = entry.IsPersistentAsset
                ? Path.GetDirectoryName(entry.Path)
                : Path.GetFileNameWithoutExtension(entry.SceneName);
            
            var locIconRect = EditorGUILayout.GetControlRect(false, GUILayout.Width(19), GUILayout.Height(19));
            if (GUI.Button(locIconRect, locContent, GUIStyle.none))
                EditorGUIUtility.PingObject(entry.Value);
            
            // Object thumbnail
            var thumbnailContent = new GUIContent
            {
                image = entry.Value != null ? AssetPreview.GetMiniThumbnail(entry.Value) : EditorGUIUtility.IconContent("_Help@2x").image,
                tooltip = entry.Type
            };

            var thumbRect = EditorGUILayout.GetControlRect(false, GUILayout.Width(19), GUILayout.Height(19));
            if (GUI.Button(thumbRect, thumbnailContent, GUIStyle.none))
                EditorGUIUtility.PingObject(entry.Value);

            Utilities.DrawVerticalSeparator();
            
            GUILayout.Label(new GUIContent(entry.Name, $"{entry.Type}"));

            if (SerializedHistory.Instance.showDebug)
            {
                var textRect = GUILayoutUtility.GetLastRect();
                GUI.Label(textRect, entry.Uses.ToString(), _rowCountStyle);
            }
            
            GUILayout.EndHorizontal();

            var entryRect = GUILayoutUtility.GetLastRect();
            int selectablePadding = 26;
            entryRect.x += selectablePadding;
            entryRect.width -= selectablePadding;

            if (entry.Value != null && GUI.Button(entryRect, GUIContent.none, GUIStyle.none))
            {
                selectedCallback?.Invoke(entry);
            }
        }
    }
}
