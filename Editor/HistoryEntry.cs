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
            
            Path = GetObjectPath(Value);
            SceneName = GetObjectScene(Value);

            Name = Value.name;
            Type = Value.GetType().Name;
            GlobalId = GetObjectGlobalId(Value);
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
            return Equals(GlobalId, other.GlobalId);
        }

        public bool Equals(Object other)
        {
            if (other == null) return false;
            return Equals(GlobalId, GetObjectGlobalId(other));
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is Object uObj) return Equals(uObj);
            return obj is HistoryEntry other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (GlobalId != null ? GlobalId.GetHashCode() : 0);
        }

        public int CompareTo(HistoryEntry other)
        {
            return Uses.CompareTo(other.Uses);
        }
        
        #region Static Helpers
        public static string GetObjectPath(Object value)
        {
            if (!EditorUtility.IsPersistent(value) && value is GameObject activeGameObject)
            {
                return SearchUtils.GetTransformPath(activeGameObject.transform);
            }

            return SearchUtils.GetObjectPath(value);
        }

        public static string GetObjectScene(Object value)
        {
            if (!EditorUtility.IsPersistent(value) && value is GameObject activeGameObject)
            {
                return activeGameObject.scene.path;
            }

            return "";
        }

        public static string GetObjectGlobalId(Object value)
        {
            return GlobalObjectId.GetGlobalObjectIdSlow(value).ToString();
        }
        #endregion
    }
}
