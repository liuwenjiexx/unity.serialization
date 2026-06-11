using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.Serialization.Tests
{
    public class TestSerialization : MonoBehaviour
    {
        public SerializableDictionary<string, string> string2StringDictionary;
        public SerializableDictionary<string, int> string2IntDictionary;
        public SerializableDictionary<int, int> int2intDictionary;
        public SerializableHashSet<string> stringHashSet;
        public SerializableHashSet<int> intHashSet;

        [ContextMenu("Add String Dictionary")]
        public void AddStringDictionary()
        {
            string key = Random.Range(0, 1000).ToString();
            string value;
            value = "str " + key;

            string2StringDictionary[key] = value;
            //EditorUtility.SetDirty(this);
        }
        [ContextMenu("Set String Dictionary 0")]
        public void SetStringDictionary()
        {
            if (string2StringDictionary.Count == 0)
                return;
            var first = string2StringDictionary.FirstOrDefault();

            var newValue = first.Value + first.Value.Length;

            string2StringDictionary[first.Key] = newValue;
        }
        [ContextMenu("Remove String Dictionary 0")]
        public void RemoveDictionary()
        {
            if (string2StringDictionary.Count == 0)
                return;
            var first = string2StringDictionary.FirstOrDefault();
            string2StringDictionary.Remove(first.Key);
        }

        [ContextMenu("Add String HashSet")]
        public void AddStringHashSet()
        {
            string value;  
            value = "str " + Random.Range(0, 1000);
            stringHashSet.Add(value);
            //EditorUtility.SetDirty(this);
        }
        [ContextMenu("Set String HashSet 0")]
        public void SetStringHashSet()
        {
            var first = stringHashSet.FirstOrDefault();

            if (first != null)
            {
                var newValue = first + first.Length;

                stringHashSet.Remove(first);
                stringHashSet.Add(newValue);
            }
        }
        [ContextMenu("Remove String HashSet 0")]
        public void RemoveStringHashSet()
        {
            var first = stringHashSet.FirstOrDefault();
            if (first != null)
                stringHashSet.Remove(first);
        }
    }
}