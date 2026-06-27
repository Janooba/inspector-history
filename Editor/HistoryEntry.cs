using System;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VoidState.InspectorHistory.Editor
{
    [Serializable]
    public class HistoryEntry : IEquatable<HistoryEntry>, IComparable<HistoryEntry>
    {
        [NonSerialized] private Object _value;
        public Object Value
        {
            get
            {
                if (_value == null)
                {
                    if (IsPersistentAsset)
                    {
                        _value = AssetDatabase.LoadAssetAtPath<Object>(Path);
                    }
                }

                return _value;
            }

            set
            {
                _value = value;
                IsPersistentAsset = EditorUtility.IsPersistent(value);
                
                if (!IsPersistentAsset && value is GameObject activeGameObject)
                {
                    SceneName = activeGameObject.scene.path;
                    Path = SearchUtils.GetTransformPath(activeGameObject.transform);
                }
                else
                {
                    SceneName = "";
                    Path = SearchUtils.GetObjectPath(value);
                }

                Name = Value.name;
                Type = Value.GetType().Name;
            }
        }
            
        public string Path = "";
        public string Name = "";
        public string Type = "";
        public int Uses;
        public bool IsFavourite;
        public bool IsPersistentAsset;
        public string SceneName = "";

        public HistoryEntry(Object value)
        {
            Value = value;
            Uses = 0;
            IsFavourite = false;
        }
        
        public bool Equals(HistoryEntry other)
        {
            if (other == null) return false;
            return Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
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
}
