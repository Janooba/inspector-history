using System;
using UnityEditor;
using UnityEngine;

namespace VoidState.InspectorHistory.Editor
{
    public static class InspectorHeader
    {
        private static NavbarView _navbar;
        
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            _navbar ??= new NavbarView(HistoryService.Instance);
            
            // Make sure it displays last
            EditorApplication.delayCall += DelayCall;
        }

        private static void DelayCall()
        {
            EditorApplication.delayCall -= DelayCall;
            UnityEditor.Editor.finishedDefaultHeaderGUI -= InjectNavbar;
            UnityEditor.Editor.finishedDefaultHeaderGUI += InjectNavbar;
        }

        private static void InjectNavbar(UnityEditor.Editor editor)
        {
            _navbar.Draw();
        }
    }
}
