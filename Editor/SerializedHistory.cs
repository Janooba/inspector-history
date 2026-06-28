using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VoidState.InspectorHistory.Editor
{
    public class SerializedHistory : ScriptableObject
    {
        private static SerializedHistory _instance;
        public static SerializedHistory Instance
        {
            get
            {
                if (!_instance)
                {
                    var guids = AssetDatabase.FindAssetGUIDs("t:SerializedHistory");
                    if (guids.Length == 0)
                    {
                        // None found, we must create it
                        _instance = CreateInstance<SerializedHistory>();
                        AssetDatabase.CreateAsset(_instance, $"Assets/Resources/SerializedHistory.asset");
                        AssetDatabase.SaveAssets();
                    }
                    else
                    {
                        if (guids.Length > 1)
                        {
                            // We have more than one? That could cause issues.
                            Debug.LogError(
                                $"There is more than one serialized history asset in this project. This could" +
                                $"cause weird issues and inconsistencies. Please ensure only one exists.");
                        }
                        
                        _instance = AssetDatabase.LoadAssetByGUID<SerializedHistory>(guids[0]);
                    }
                }

                return _instance;
            }
        }

        public List<HistoryEntry> history = new List<HistoryEntry>();
    }
}
