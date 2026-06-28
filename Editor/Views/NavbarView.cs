using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VoidState.InspectorHistory.Editor
{
    public class NavbarView
    {
        private const int NAV_BTN_HEIGHT = 25;
        private const int NAV_BTN_WIDTH = 30;

        public NavbarView(Func<bool> backDisabledCheck, Func<bool> forwardDisabledCheck, Action backClicked, Action forwardClicked)
        {
            IsBackDisabled = backDisabledCheck;
            IsForwardDisabled = forwardDisabledCheck;
            BackClicked = backClicked;
            ForwardClicked = forwardClicked;
            
            _iconBack = EditorGUIUtility.IconContent("back@2x");
            _iconForward = EditorGUIUtility.IconContent("forward@2x");
        }
        
        private GUIContent _iconBack;
        private GUIContent _iconForward;

        private Func<bool> IsBackDisabled;
        private Func<bool> IsForwardDisabled;
        
        private Action BackClicked;
        private Action ForwardClicked;
        
        public void Draw(List<HistoryEntry> rawHistory, HistoryEntry selectedEntry)
        {
            GUIStyle backButtonStyle = new GUIStyle(EditorStyles.miniButtonLeft)
            {
                fixedHeight = NAV_BTN_HEIGHT
            };

            GUIStyle forwardButtonStyle = new GUIStyle(EditorStyles.miniButtonRight)
            {
                fixedHeight = NAV_BTN_HEIGHT
            };

            GUIStyle pathBoxStyle = new GUIStyle(EditorStyles.textField)
            {
                alignment = TextAnchor.MiddleRight,
            };

            using (new GUILayout.HorizontalScope())
            {
                string path = selectedEntry != null ? selectedEntry.Path : "";
                
                GUILayout.Label($"History Size: {rawHistory.Count}", GUILayout.Height(NAV_BTN_HEIGHT));
                var labelZone = GUILayoutUtility.GetLastRect();
                
                //EditorGUI.DrawRect(labelZone, Color.magenta.ChangeAlpha(0.2f));

                // if (GUI.Button(labelZone, new GUIContent(path, path), pathBoxStyle))
                // {
                //     
                // }
                
                
                
                EditorGUI.BeginDisabledGroup(IsBackDisabled());
                if (GUILayout.Button(_iconBack, backButtonStyle, GUILayout.Height(NAV_BTN_HEIGHT), GUILayout.Width(NAV_BTN_WIDTH)))
                {
                    BackClicked?.Invoke();
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(IsForwardDisabled());
                if (GUILayout.Button(_iconForward, forwardButtonStyle, GUILayout.Height(NAV_BTN_HEIGHT), GUILayout.Width(NAV_BTN_WIDTH)))
                {
                    ForwardClicked?.Invoke();
                }

                EditorGUI.EndDisabledGroup();
            }
        }

        private void DrawBreadcrumbs(Rect zone, string path)
        {
            // Breadcrumbs
            string[] folders = path.Split('/');

            GUIStyle crumbStyle = new GUIStyle(EditorStyles.label)
            {
                fixedHeight = NAV_BTN_HEIGHT,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = false,
                clipping = TextClipping.Clip,
                padding = new RectOffset(4, 4, 4, 4)
            };

            float x = zone.width;
            for (var i = folders.Length - 2; i >= 0; i--)
            {
                var folder = folders[i];
                Vector2 size = crumbStyle.CalcSize(new GUIContent(folder));
                size.x = Math.Min(size.x, 80);
                x -= size.x;
                    
                var rect = new Rect(x, 2, size.x, NAV_BTN_HEIGHT);

                EditorGUI.DrawRect(rect, Color.black.ChangeAlpha(0.2f));
                GUI.Box(rect, folder, crumbStyle);

                x -= 12;
                rect = new Rect(x + 2, 0, 10, 20);
                    
                GUI.DrawTexture(rect, _iconForward.image);
            }
        }
    }
}
