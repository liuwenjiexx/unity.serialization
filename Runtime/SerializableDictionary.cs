using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Unity.Serialization
{
    //基类保持 Dictionary，透明化支持序列化
    [Serializable]
    [DebuggerDisplay("Count = {Count}")]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver, ISerializableDictionary
    {
        [SerializeField]
        private List<TKey> keys;
        [SerializeField]
        private List<TValue> values;

        public SerializableDictionary()
        { }

        public SerializableDictionary(Dictionary<TKey, TValue> dic)
        {
            foreach (var pair in dic)
            {
                this[pair.Key] = pair.Value;
            }
        }

        public Dictionary<TKey, TValue> Dictionary => this;

        public void OnBeforeSerialize()
        {
            if (keys == null) keys = new();
            if (values == null) values = new();
            keys.Clear();
            values.Clear();

            foreach (var pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();
            if (keys != null)
            {
                for (int i = 0, count = keys.Count; i < count; i++)
                    this[keys[i]] = values[i];
            }
        }
    }

    //PropertyDrawer
    internal interface ISerializableDictionary
    {

    }
}
