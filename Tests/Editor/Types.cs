using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Serialization.EditorTests
{
    [Serializable]
    class BaseType
    {
        public string str;

        private string ignorePrivateField;

        [NonSerialized]
        public string ignorePublicField;

        public int int32;
        public long int64;

        public float float32;
        public double float64;


        public EnumValue enumValue;
    }

    [Serializable]
    class ArrayMemberType
    {
        public BaseType[] array;
        public List<BaseType> list;
    }

    public enum EnumValue
    {
        None,
        One = 1,
        Two = 2,
    }

    [Serializable]
    public class PropertyClass
    {
        public int Int32 { get; set; }

        public string Str { get; set; }

        public string InogreNoneSet { get; }
    }
}
