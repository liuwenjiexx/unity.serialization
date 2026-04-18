using System;
using UnityEngine;

namespace Unity.Serialization
{
    public class SerializableObjectDroplistAttribute : PropertyAttribute
    {
        public SerializableObjectDroplistAttribute()
        {
        }
        public SerializableObjectDroplistAttribute(Type baseType)
        {
            this.BaseType = baseType;
        }
        public Type BaseType { get; set; }
    }
}
