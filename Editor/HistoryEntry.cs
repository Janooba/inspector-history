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
                if (_value == null) TryGetReference();
                return _value;
            }
            set
            {
                _value = value;
                UpdateMetadata();
            }
        }
            
        public string Path = "";
        public string Name = "";
        public string Type = "";
        public string GlobalId;
        public int Uses;
        public bool IsFavourite;
        public bool IsPersistentAsset;
        public string SceneName = "";

        public bool IsUnresolved => Value == null;

        public HistoryEntry(Object value)
        {
            Value = value;
            Uses = 0;
            IsFavourite = false;
        }

        public GlobalObjectId ResolveGlobalId()
        {
            GlobalObjectId.TryParse(GlobalId, out var id);
            return id;
        }
        
        public void UpdateMetadata()
        {
            if (Value == null) return;
            
            IsPersistentAsset = EditorUtility.IsPersistent(Value);
                
            if (!IsPersistentAsset && Value is GameObject activeGameObject)
            {
                SceneName = activeGameObject.scene.path;
                Path = SearchUtils.GetTransformPath(activeGameObject.transform);
            }
            else
            {
                SceneName = "";
                Path = SearchUtils.GetObjectPath(Value);
            }

            Name = Value.name;
            Type = Value.GetType().Name;
            GlobalId = GlobalObjectId.GetGlobalObjectIdSlow(Value).ToString();
        }

        public void TryGetReference()
        {
            if (!_value && GlobalObjectId.TryParse(GlobalId, out var id))
            {
                _value = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);   
            }

            if (_value) UpdateMetadata();
        }
        
        public bool Equals(HistoryEntry other)
        {
            if (other == null) return false;
            return Equals(Path, other.Path);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            return obj is HistoryEntry other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Path != null ? Path.GetHashCode() : 0);
        }

        public int CompareTo(HistoryEntry other)
        {
            return Uses.CompareTo(other.Uses);
        }
    }
}
