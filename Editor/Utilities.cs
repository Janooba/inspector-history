using UnityEditor;
using UnityEngine;

namespace VoidState.InspectorHistory.Editor
{
    public static class Utilities
    {
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

        public static void DrawTitle(string title)
        {
            var titleRect = EditorGUILayout.GetControlRect(false, 24, GUIStyle.none);
            EditorGUI.DrawRect(titleRect, Color.black.ChangeAlpha(0.2f));
            titleRect.x += 5;
            EditorGUI.LabelField(titleRect, title, EditorStyles.boldLabel);
        }
        
        public static void DrawSeparator()
        {
            var spaceRect = EditorGUILayout.GetControlRect(false, 10);
            var separatorRect = new Rect(spaceRect);
            separatorRect.height = 2;
            separatorRect.y = spaceRect.center.y;
            
            EditorGUI.DrawRect(separatorRect, Color.black.ChangeAlpha(0.2f));
        }
        
        public static Color ChangeAlpha(this Color color, float newAlpha)
        {
            return new Color(color.r, color.g, color.b, newAlpha);
        }
    }
}
