using System;
using UnityEngine;

namespace Unity.Serialization
{
    /// <summary>
    /// 
    /// </summary>
    /// <example>
    ///  [SerializableObjectDroplist]
    ///  private SerializableObject<T> value;
    /// </example>
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
