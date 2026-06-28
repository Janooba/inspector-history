using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VoidState.InspectorHistory.Editor
{
    public static class EntryView
    {
        private static GUIContent _iconStar;
        private static GUIContent _iconStarSelected;
        private static GUIContent _iconWarning;
        
        private static Texture2D _highlightTex;
        private static GUIStyle _highlightRowStyle;
        
        private static Texture2D _missingTex;
        private static GUIStyle _missingStyle;
        
        private static GUIStyle _rowStyle;
        private static GUIStyle _rowButton;
        private static GUIStyle _rowButtonHighlight;
        private static GUIStyle _rowCountStyle;

        private static void EnsureStylesInitialized()
        {
            _highlightTex ??= Utilities.MakeTex(2, 2, new Color(0f, 1f, 1f, 0.2f));
            _missingTex ??= Utilities.MakeTex(2, 2, new Color(1f, 0f, 0f, 0.2f));

            _rowStyle ??= new GUIStyle();
            _rowStyle.fixedHeight = 22;
            _highlightRowStyle ??= new GUIStyle(_rowStyle)
            {
                normal = { background = _highlightTex }
            };

            _missingStyle ??= new GUIStyle(_rowStyle)
            {
                normal = { background = _missingTex }
            };

            _rowButton ??= new GUIStyle(EditorStyles.iconButton);
            _rowButton.fixedHeight = 22;
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

            if (entry.Value == null)
                GUILayout.BeginHorizontal(_missingStyle);
            else 
                GUILayout.BeginHorizontal(isSelected ? _highlightRowStyle : _rowStyle);

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

            var textRect = GUILayoutUtility.GetLastRect();
            GUI.Label(textRect, entry.Uses.ToString(), _rowCountStyle);
            
            GUILayout.EndHorizontal();

            int selectablePadding = 26;
            var entryRect = GUILayoutUtility.GetLastRect();
            entryRect.x += selectablePadding;
            entryRect.width -= selectablePadding;

            if (entry.Value != null && GUI.Button(entryRect, GUIContent.none, GUIStyle.none))
            {
                selectedCallback?.Invoke(entry);
            }
        }
    }
}
