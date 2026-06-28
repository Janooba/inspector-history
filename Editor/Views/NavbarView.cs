using System;
using UnityEditor;
using UnityEngine;

namespace VoidState.InspectorHistory.Editor
{
    public class NavbarView
    {
        private const int NAV_BTN_HEIGHT = 25;
        private const int NAV_BTN_WIDTH = 30;

        private HistoryService _service;
        
        public NavbarView(HistoryService service)
        {
            _service = service;
            _iconBack = EditorGUIUtility.IconContent("back@2x");
            _iconForward = EditorGUIUtility.IconContent("forward@2x");
        }
        
        private GUIContent _iconBack;
        private GUIContent _iconForward;
        
        public void Draw()
        {
            GUIStyle backButtonStyle = new GUIStyle(EditorStyles.miniButtonLeft)
            {
                fixedHeight = NAV_BTN_HEIGHT
            };

            GUIStyle forwardButtonStyle = new GUIStyle(EditorStyles.miniButtonRight)
            {
                fixedHeight = NAV_BTN_HEIGHT
            };

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label($"History Size: {_service.HistoryEntries.Count}", GUILayout.Height(NAV_BTN_HEIGHT));
                
                EditorGUI.BeginDisabledGroup(!_service.CanGoBack);
                if (GUILayout.Button(_iconBack, backButtonStyle, GUILayout.Height(NAV_BTN_HEIGHT), GUILayout.Width(NAV_BTN_WIDTH)))
                {
                    _service.GoBack();
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(!_service.CanGoForward);
                if (GUILayout.Button(_iconForward, forwardButtonStyle, GUILayout.Height(NAV_BTN_HEIGHT), GUILayout.Width(NAV_BTN_WIDTH)))
                {
                    _service.GoForward();
                }

                EditorGUI.EndDisabledGroup();
            }
        }
    }
}
