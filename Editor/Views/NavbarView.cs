using System;
using UnityEditor;
using UnityEngine;

namespace VoidState.InspectorHistory.Editor
{
    public class NavbarView
    {
        private const int NAV_BTN_HEIGHT = 20;
        private const int NAV_BTN_WIDTH = 30;

        private HistoryService _service;
        
        public NavbarView(HistoryService service)
        {
            _service = service;
            _iconBack = EditorGUIUtility.IconContent("back");
            _iconForward = EditorGUIUtility.IconContent("forward");
            _iconConfig =  EditorGUIUtility.IconContent("_Popup@2x");
        }
        
        private GUIContent _iconBack;
        private GUIContent _iconForward;
        private GUIContent _iconConfig;
        
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
            
            GUIStyle configButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fixedHeight = NAV_BTN_HEIGHT
            };

            // Readonly assets may disable their header which includes this.
            // We still want to navigate though, so we must override it
            bool originalEnabledState = GUI.enabled;
            GUI.enabled = true; 
            
            using (new GUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledGroupScope(!_service.CanGoBack))
                {
                    if (GUILayout.Button(_iconBack, backButtonStyle, GUILayout.Height(NAV_BTN_HEIGHT),
                            GUILayout.Width(NAV_BTN_WIDTH)))
                    {
                        _service.GoBack();
                    }
                }

                using (new EditorGUI.DisabledGroupScope(!_service.CanGoForward))
                {
                    if (GUILayout.Button(_iconForward, forwardButtonStyle, GUILayout.Height(NAV_BTN_HEIGHT),
                            GUILayout.Width(NAV_BTN_WIDTH)))
                    {
                        _service.GoForward();
                    }
                }
                
                GUILayout.FlexibleSpace();

                if (GUILayout.Button(_iconConfig, configButtonStyle,
                    GUILayout.Height(NAV_BTN_HEIGHT),
                    GUILayout.Width(NAV_BTN_WIDTH)))
                {
                    Selection.SetActiveObjectWithContext(SerializedHistory.Instance, null);
                }
            }

            GUI.enabled = originalEnabledState;
        }
    }
}
