using UnityEditor;
using UnityEngine;

namespace VoidState.InspectorHistory.Editor
{
    public static class Utilities
    {
        public const string PACKAGE_PATH = "Packages/com.voidstate.inspector-history";
        public const string PREFS_PREFIX = "inspector_history";
        
        public static Texture2D MakeTex(int width, int height, Color col)
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

        public static Rect DrawTitle(string title)
        {
            var titleRect = EditorGUILayout.GetControlRect(false, 24, GUIStyle.none);
            EditorGUI.DrawRect(titleRect, Color.black.ChangeAlpha(0.2f));

            int padding = 5;
            var rectPadding = new RectOffset(padding, padding, padding, padding);
            var labelRect = rectPadding.Remove(titleRect);
            EditorGUI.LabelField(labelRect, title, EditorStyles.boldLabel);
            //EditorGUI.DrawRect(labelRect, Color.magenta.ChangeAlpha(0.2f));

            return titleRect;
        }

        public static Rect DrawTitleFoldout(string title, ref bool isExpanded)
        {
            var titleRect = EditorGUILayout.GetControlRect(false, 24, GUIStyle.none);
            EditorGUI.DrawRect(titleRect, Color.black.ChangeAlpha(0.2f));
            
            isExpanded = GUI.Toggle(titleRect, isExpanded, GUIContent.none, EditorStyles.foldout);
            
            int padding = 5;
            var rectPadding = new RectOffset(padding * 3, padding, padding, padding);
            var labelRect = rectPadding.Remove(titleRect);
            EditorGUI.LabelField(labelRect, title, EditorStyles.boldLabel);
            //EditorGUI.DrawRect(labelRect, Color.magenta.ChangeAlpha(0.2f));

            return titleRect;
        }
        
        public static void DrawSeparator()
        {
            var spaceRect = EditorGUILayout.GetControlRect(false, 10);
            var separatorRect = new Rect(spaceRect);
            separatorRect.height = 2;
            separatorRect.y = spaceRect.center.y;
            
            EditorGUI.DrawRect(separatorRect, Color.black.ChangeAlpha(0.2f));
        }
        
        public static void DrawVerticalSeparator(float padding = 5)
        {
            var spaceRect = EditorGUILayout.GetControlRect(false, GUILayout.Width(padding));
            var separatorRect = new Rect(spaceRect);
            separatorRect.width = 1;
            separatorRect.x = spaceRect.center.x;
            
            EditorGUI.DrawRect(separatorRect, Color.black.ChangeAlpha(0.2f));
        }
        
        public static Color ChangeAlpha(this Color color, float newAlpha)
        {
            return new Color(color.r, color.g, color.b, newAlpha);
        }
    }
}
