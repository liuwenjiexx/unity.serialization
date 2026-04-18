using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using Unity.Serialization;
using Object = UnityEngine.Object;

namespace Unity
{
    [Obsolete]
    [Serializable]
    public class Serializable<T> : ISerializationCallbackReceiver
    {
        private T target;

        // Value type
        [SerializeField]
        SerializableValue v;

        // Reference type
        [SerializeField]
        private List<SerializableObject3> r;
        private Exception deserializeError;

        [SerializeField]
        private SerializableOptions o;

        public Serializable(SerializableOptions options = SerializableOptions.Field)
        {
            this.o = options;
            target = default;
            v = SerializableValue.Null;
            r = null;
            deserializeError = null;

        }

        public Serializable(T target, SerializableOptions options = SerializableOptions.Field)
        {
            this.target = target;
            this.o = options;
            v = SerializableValue.Null;
            r = null;
            deserializeError = null;
        }

        public T Target
        {
            get
            {
                //if (!deserializeError)
                //{
                //target = default;

                //if (!string.IsNullOrEmpty(typeName))
                //{
                //    Type type = Type.GetType(typeName);
                //    if (type != null)
                //    {
                //        // target = (T)JsonUtility.FromJson(data, type);
                //    }
                //}
                //}
                return target;
            }

            set
            {
                target = value;
                deserializeError = null;
            }
        }

        public Exception DeserializeError => deserializeError;

        public SerializableOptions Options { get => o; set => o = value; }

        public void OnBeforeSerialize()
        {
            if (deserializeError != null)
                return;

            v = SerializableValue.Null;
            if (r != null) r.Clear();

            Type targetType = target != null ? target.GetType() : null;
            var typeCode = SerializableUtility.TypeToSerializableTypeCode(targetType);

            if (SerializableUtility.IsBaseType(typeCode))
            {
                v = SerializeBaseValue(typeCode, target);
                r = null;
            }
            else
            {
                if (r == null) r = new();
                v = new SerializableValue(typeCode);
                SerializableObject3 root;
                root = new SerializableObject3(targetType, typeof(T));
                r.Add(root);

                if (target != null)
                {
                    SerializeValue(ref root, target, typeof(T));
                    r[0] = root;
                }
            }

        }

        public void OnAfterDeserialize()
        {
            Deserialize();
        }

        void Deserialize()
        {
            try
            {
                deserializeError = null;
                target = default;

                if (SerializableUtility.IsBaseType(v.c))
                {
                    target = (T)DeserializeBaseValue(v);
                }
                else if (r != null && r.Count > 0)
                {
                    var root = r[0];
                    var targetValue = Deserialize(root, typeof(T));
                    if (targetValue != null)
                    {
                        if (targetValue is T)
                        {
                            target = (T)targetValue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                deserializeError = ex;
            }
        }



        void SerializeValue(ref SerializableObject3 @object, object target, Type hintType = null)
        {
            if (target == null)
            {
                //@object.v = SerializableValue.Null;
                @object.SetNull();
                return;
            }

            SerializableTypeCode typeCode = @object.c;

            if (@object.m == null) @object.m = new();

            if (@object.c == SerializableTypeCode.Object)
            {
                SerializeObject(ref @object, target);
            }
            else if ((typeCode & SerializableTypeCode.ArrayOrList) != 0)
            {
                SerializeArray(ref @object, target, hintType);
            }
        }


        SerializableValue SerializeBaseValue(SerializableTypeCode typeCode, object target)
        {
            SerializableValue value = new SerializableValue(typeCode);
            switch (typeCode)
            {
                case SerializableTypeCode.UnityObject:
                    Object unityObject = target as Object;
                    value.o = unityObject;
                    break;
                case SerializableTypeCode.Int32:
                    value.i = target == null ? 0L : (int)target;
                    break;
                case SerializableTypeCode.Int64:
                    value.i = target == null ? 0L : (long)target;
                    break;
                case SerializableTypeCode.Single:
                    value.f = target == null ? 0d : (float)target;
                    break;
                case SerializableTypeCode.Double:
                    value.f = target == null ? 0d : (double)target;
                    break;
                default:
                    value.s = SerializeToString(value.c, target);
                    break;
            }
            return value;
        }
        void SerializeObject(ref SerializableObject3 @object, object target)
        {
            if (target == null)
            {
                //@object.v = SerializableValue.Null;
                @object.SetNull();
                return;
            }

            Type targetType = target.GetType();
            object value;
            Type valueType;
            SerializableTypeCode typeCode;
            if (@object.m == null)
                @object.m = new();
            foreach (var member in SerializableMemberInfo.GetMembers(targetType).Values)
            {
                try
                {
                    typeCode = member.typeCode;
                    if (member.field != null)
                    {
                        if ((o & SerializableOptions.Field) != SerializableOptions.Field)
                            continue;
                        value = member.field.GetValue(target);
                    }
                    else
                    {
                        if ((o & SerializableOptions.Property) != SerializableOptions.Property)
                            continue;
                        value = member.property.GetValue(target);
                    }

                    valueType = value != null ? value.GetType() : null;

                    if (typeCode == SerializableTypeCode.Object || (typeCode & SerializableTypeCode.ArrayOrList) != 0)
                    {
                        typeCode = SerializableUtility.TypeToSerializableTypeCode(valueType);
                    }

                    if (valueType == null)
                    {
                        @object.m.Add(new SerializableMember(member.name, SerializableTypeCode.Null));
                    }
                    else if ((typeCode & SerializableTypeCode.ArrayOrList) != 0)
                    {
                        SerializableObject3 array;
                        array = new SerializableObject3(valueType, member.valueType);
                        if (array.m == null) array.m = new();
                        int refIndex = r.Count;
                        r.Add(array);
                        @object.m.Add(new SerializableMember(member.name, refIndex));
                        SerializeArray(ref array, value, member.valueType);
                        r[refIndex] = array;
                    }
                    else if (typeCode == SerializableTypeCode.Object)
                    {
                        SerializableObject3 next = new SerializableObject3(valueType, member.valueType);
                        if (next.m == null) next.m = new();
                        int refIndex = r.Count;
                        r.Add(next);
                        @object.m.Add(new SerializableMember(member.name, refIndex));
                        SerializeObject(ref next, value);
                        r[refIndex] = next;
                    }
                    else
                    {
                        @object.m.Add(new SerializableMember(member.name, SerializeBaseValue(typeCode, value)));
                    }
                }
                catch
                {
                    Debug.LogError($"Serialize error '{targetType.FullName}.{member.name}'");
                    throw;
                }
            }
        }

        void SerializeArray(ref SerializableObject3 array, object target, Type hintType = null)
        {
            var it = target as IEnumerable;
            if (it != null)
            {
                Type hintItemType = null;
                if (hintType != null)
                {
                    if (hintType.IsArray)
                    {
                        hintItemType = hintType.GetElementType();
                    }
                    else
                    {
                        var listType = hintType.FindGenericTypeDefinition(typeof(IList<>));
                        if (listType != null)
                        {
                            hintItemType = listType.GetGenericArguments()[0];
                        }
                    }
                }

                if (array.m == null)
                    array.m = new();

                foreach (var item in it)
                {
                    Type itemValueType = item?.GetType();
                    var itemTypeCode = SerializableUtility.TypeToSerializableTypeCode(itemValueType);
                    if (SerializableUtility.IsBaseType(itemTypeCode))
                    {
                        array.m.Add(new SerializableMember(null, SerializeBaseValue(itemTypeCode, item)));
                    }
                    else
                    {
                        var itemObject = new SerializableObject3(itemValueType, hintItemType);
                        int refIndex = this.r.Count;
                        this.r.Add(itemObject);
                        array.m.Add(new SerializableMember(refIndex));
                        SerializeValue(ref itemObject, item);
                        r[refIndex] = itemObject;
                    }
                }
            }
        }

        //SerializableValue BaseSerializeValue(SerializableTypeCode typeCode, object target)
        //{
        //    switch (typeCode)
        //    {
        //        case SerializableTypeCode.UnityObject:
        //            Object unityObject = target as Object;
        //            return new SerializableValue(unityObject);
        //    }
        //    return new SerializableValue(typeCode, SerializeToString(typeCode, target));
        //}


        internal static string SerializeToString(SerializableTypeCode typeCode, object value)
        {
            string str = null;


            switch (typeCode)
            {
                case SerializableTypeCode.String:
                    str = value as string;
                    break;
                case SerializableTypeCode.Vector2:
                    {
                        Vector2 v = (Vector2)value;
                        str = v.x.ToString() + "," + v.y.ToString();
                    }
                    break;
                case SerializableTypeCode.Vector2Int:
                    {
                        Vector2Int v = (Vector2Int)value;
                        str = v.x.ToString() + "," + v.y.ToString();
                    }
                    break;
                case SerializableTypeCode.Vector3:
                    {
                        Vector3 v = (Vector3)value;
                        str = v.x + "," + v.y + "," + v.z;
                    }
                    break;
                case SerializableTypeCode.Vector3Int:
                    {
                        Vector3Int v = (Vector3Int)value;
                        str = v.x + "," + v.y + "," + v.z;
                    }
                    break;
                case SerializableTypeCode.Vector4:
                    {
                        Vector4 v = (Vector4)value;
                        str = v.x + "," + v.y + "," + v.z + "," + v.w;
                    }
                    break;
                case SerializableTypeCode.Color:
                    {
                        Color v = (Color)value;
                        str = v.r + "," + v.g + "," + v.b + "," + v.a;
                    }
                    break;
                case SerializableTypeCode.Color32:
                    {
                        Color32 v = (Color32)value;
                        str = v.r + "," + v.g + "," + v.b + "," + v.a;
                    }
                    break;
                case SerializableTypeCode.Rect:
                    {
                        Rect v = (Rect)value;
                        str = v.x + "," + v.y + "," + v.width + "," + v.height;
                    }
                    break;
                case SerializableTypeCode.RectInt:
                    {
                        RectInt v = (RectInt)value;
                        str = v.x + "," + v.y + "," + v.width + "," + v.height;
                    }
                    break;
                case SerializableTypeCode.RectOffset:
                    {
                        RectOffset v = value as RectOffset;
                        if (v == null)
                            v = new RectOffset();
                        str = v.left + "," + v.right + "," + v.top + "," + v.bottom;
                    }
                    break;
                case SerializableTypeCode.RectOffsetSerializable:
                    {
                        SerializableRectOffset v = value as SerializableRectOffset;
                        if (v == null)
                            v = new SerializableRectOffset();
                        str = v.left + "," + v.right + "," + v.top + "," + v.bottom;
                    }
                    break;
                case SerializableTypeCode.AnimationCurve:
                    AnimationCurve curve = (AnimationCurve)value;
                    str = ToCurveString(curve);
                    break;
                default:

                    if (value != null)
                    {
                        if (value is Enum)
                        {
                            if (typeof(int).IsInstanceOfType(value))
                            {
                                //str = ((int)value).ToString();
                                str = Convert.ToInt32(value).ToString();
                            }
                            else
                            {
                                //str = ((long)value).ToString();
                                str = Convert.ToInt64(value).ToString();
                            }
                        }
                        else
                        {
                            str = value.ToString();
                        }
                    }
                    break;
            }
            return str;
        }

        object Deserialize(SerializableObject3 @object, Type hintType)
        {
            SerializableTypeCode typeCode = @object.c;
            object value = null;
            if (typeCode == SerializableTypeCode.Object)
            {
                value = DeserializeObject(@object, hintType);
            }
            else if ((typeCode & SerializableTypeCode.ArrayOrList) != 0)
            {
                value = DeserializeArray(@object, hintType);
            }
            return value;
        }

        object DeserializeBaseValue(SerializableValue memberValue, Type hintType)
        {
            var value = DeserializeBaseValue(memberValue);

            if (value == null)
            {
                return hintType.DefaultValue();
            }

            if (hintType != value.GetType() && !hintType.IsAssignableFrom(value.GetType()))
            {
                try
                {
                    if (hintType.IsEnum)
                    {
                        if (value is string)
                            value = Enum.Parse(hintType, (string)value);
                        else if (value is byte)
                            value = Enum.ToObject(hintType, (byte)value);
                        else if (value is short)
                            value = Enum.ToObject(hintType, (short)value);
                        else if (value is int)
                            value = Enum.ToObject(hintType, (int)value);
                    }
                    else
                    {
                        value = Convert.ChangeType(value, hintType);
                    }
                }
                catch { }
            }
            return value;
        }
        object DeserializeBaseValue(SerializableValue value)
        {
            switch (value.c)
            {
                case SerializableTypeCode.UnityObject:
                    return value.o;
                case SerializableTypeCode.Byte:
                    return (byte)value.i;
                case SerializableTypeCode.SByte:
                    return (sbyte)value.i;
                case SerializableTypeCode.Int16:
                    return (short)value.i;
                case SerializableTypeCode.UInt16:
                    return (ushort)value.i;
                case SerializableTypeCode.Int32:
                    return (int)value.i;
                case SerializableTypeCode.UInt32:
                    return (uint)value.i;
                case SerializableTypeCode.Int64:
                    return value.i;
                case SerializableTypeCode.UInt64:
                    return (uint)value.i;
                case SerializableTypeCode.Single:
                    return (float)value.f;
                case SerializableTypeCode.Double:
                    return value.f;
            }
            return DeserializeFromString(value.c, value.s);
        }


        object DeserializeObject(SerializableObject3 @object, Type hintType)
        {
            if (@object.c == SerializableTypeCode.Null)
                return null;

            object obj = null;
            Type targetType = null;
            if (!string.IsNullOrEmpty(@object.t))
            {
                targetType = Type.GetType(@object.t);
                if (targetType == null)
                    throw new Exception($"Not found type: '{@object.t}'");
                obj = Activator.CreateInstance(targetType);
            }
            else
            {
                targetType = hintType;
            }

            if (targetType != null)
            {
                obj = Activator.CreateInstance(targetType);
            }
            if (obj == null)
                return null;

            if (@object.m != null)
            {
                SerializableMember memberValue;
                object value;
                foreach (var member in SerializableMemberInfo.GetMembers(targetType).Values)
                {
                    if (!@object.FindMember(member.name, out memberValue)) continue;

                    if (memberValue._ >= 0)
                    {
                        var memberObj = r[memberValue._];
                        value = Deserialize(memberObj, member.valueType);
                    }
                    else
                    {
                        value = DeserializeBaseValue(memberValue.v, member.valueType);
                    }
                    if (value == null)
                    {
                        value = member.valueType.DefaultValue();
                    }
                    else
                    {
                        if (!member.valueType.IsAssignableFrom(value.GetType()))
                            continue;
                    }
                    if (member.field != null)
                    {
                        member.field.SetValue(obj, value);
                    }
                    else
                    {
                        if (member.property.CanWrite)
                        {
                            member.property.SetValue(obj, value, null);
                        }
                        else
                        {
                            if (value != null && value is IEnumerable)
                            {
                                var list = member.property.GetValue(obj, null) as IList;
                                if (list != null)
                                {
                                    foreach (var it in (IEnumerable)value)
                                    {
                                        list.Add(it);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return obj;
        }

        object DeserializeArray(SerializableObject3 array, Type hintType)
        {
            Type itemType = null;
            SerializableTypeCode itemTypeCode = array.c & ~SerializableTypeCode.Array;


            if (hintType.IsArray)
            {
                itemType = hintType.GetElementType();
                Array arrayValue = Array.CreateInstance(itemType, array.m != null ? array.m.Count : 0);
                if (array.m != null)
                {
                    object itemValue;
                    int index = 0;
                    foreach (var itemMemberValue in array.m)
                    {
                        itemValue = null;
                        if (itemMemberValue._ >= 0)
                        {
                            itemValue = Deserialize(r[itemMemberValue._], itemType);
                        }
                        else
                        {
                            itemValue = DeserializeBaseValue(itemMemberValue.v, itemType);
                        }
                        arrayValue.SetValue(itemValue, index++);
                    }
                }
                return arrayValue;
            }
            else
            {

                IList list = null;
                if (!string.IsNullOrEmpty(array.t))
                {
                    Type type = Type.GetType(array.t);
                    if (type != null)
                    {
                        list = Activator.CreateInstance(type) as IList;
                    }
                }
                if (list == null)
                {
                    if (hintType != null && !hintType.IsAbstract)
                    {
                        list = Activator.CreateInstance(hintType) as IList;
                    }
                }

                if (list == null)
                {
                    var listType = hintType.FindGenericTypeDefinition(typeof(IList<>));
                    if (listType != null)
                    {
                        itemType = listType.GenericTypeArguments[0];
                        list = Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType)) as IList;
                    }
                }

                if (itemType == null)
                {
                    var listType = hintType.FindGenericTypeDefinition(typeof(IList<>));
                    if (listType != null)
                    {
                        itemType = listType.GenericTypeArguments[0];
                    }
                }

                if (list != null && array.m != null)
                {
                    object itemValue;
                    foreach (var itemMemberValue in array.m)
                    {
                        itemValue = null;
                        if (itemMemberValue._ >= 0)
                        {
                            itemValue = Deserialize(r[itemMemberValue._], itemType);
                        }
                        else
                        {
                            itemValue = DeserializeBaseValue(itemMemberValue.v, itemType);
                        }
                        list.Add(itemValue);
                    }
                }
                return list;
            }

            //return hintType.DefaultValue();
        }

        internal static object DeserializeFromString(SerializableTypeCode typeCode, string str)
        {
            object value = null;
            switch (typeCode)
            {
                case SerializableTypeCode.String:
                    value = str;
                    break;
                case SerializableTypeCode.Double:
                    {
                        double n;
                        if (double.TryParse(str, out n))
                            value = n;
                        else
                            value = 0d;
                    }
                    break;
                case SerializableTypeCode.Single:
                    {
                        float n;
                        if (float.TryParse(str, out n))
                            value = n;
                        else
                            value = 0f;
                    }
                    break;
                case SerializableTypeCode.Int32:
                    {
                        int n;
                        if (int.TryParse(str, out n))
                            value = n;
                        else
                            value = 0;
                    }
                    break;
                case SerializableTypeCode.Int64:
                    {
                        long n;
                        if (long.TryParse(str, out n))
                            value = n;
                        else
                            value = 0;
                    }
                    break;
                case SerializableTypeCode.Boolean:
                    {
                        bool b;
                        if (bool.TryParse(str, out b))
                            value = b;
                        else
                            value = false;
                    }
                    break;
                case SerializableTypeCode.Vector2:
                    {
                        Vector2 v = new Vector2();
                        float n;
                        string[] parts = str.Split(',');
                        for (int i = 0; i < parts.Length; i++)
                        {
                            if (float.TryParse(parts[i], out n))
                                v[i] = n;
                        }
                        value = v;
                    }
                    break;
                case SerializableTypeCode.Vector2Int:
                    {
                        Vector2Int v = new Vector2Int();
                        int n;
                        string[] parts = str.Split(',');
                        for (int i = 0; i < parts.Length; i++)
                        {
                            if (int.TryParse(parts[i], out n))
                                v[i] = n;
                        }
                        value = v;
                    }
                    break;
                case SerializableTypeCode.Vector3:
                    {
                        Vector3 v = new Vector3();
                        float n;
                        string[] parts = str.Split(',');
                        for (int i = 0; i < parts.Length; i++)
                        {
                            if (float.TryParse(parts[i], out n))
                                v[i] = n;
                        }
                        value = v;
                    }
                    break;
                case SerializableTypeCode.Vector3Int:
                    {
                        Vector3Int v = new Vector3Int();
                        int n;
                        string[] parts = str.Split(',');
                        for (int i = 0; i < parts.Length; i++)
                        {
                            if (int.TryParse(parts[i], out n))
                                v[i] = n;
                        }
                        value = v;
                    }
                    break;
                case SerializableTypeCode.Vector4:
                    {
                        Vector4 v = new Vector4();
                        float n;
                        string[] parts = str.Split(',');
                        for (int i = 0; i < parts.Length; i++)
                        {
                            if (float.TryParse(parts[i], out n))
                                v[i] = n;
                        }
                        value = v;
                    }
                    break;
                case SerializableTypeCode.Color:
                    {
                        Color v = new Color();
                        float n;
                        string[] parts = str.Split(',');
                        for (int i = 0; i < parts.Length; i++)
                        {
                            if (float.TryParse(parts[i], out n))
                                v[i] = n;
                        }
                        value = v;
                    }
                    break;
                case SerializableTypeCode.Color32:
                    {
                        Color32 v = new Color32();
                        byte n;
                        string[] parts = str.Split(',');
                        for (int i = 0; i < parts.Length; i++)
                        {
                            if (byte.TryParse(parts[i], out n))
                            {
                                switch (i)
                                {
                                    case 0:
                                        v.r = n;
                                        break;
                                    case 1:
                                        v.g = n;
                                        break;
                                    case 2:
                                        v.b = n;
                                        break;
                                    case 3:
                                        v.a = n;
                                        break;
                                }
                            }
                        }
                        value = v;
                    }
                    break;
                case SerializableTypeCode.Rect:
                    {
                        Rect v = new Rect();
                        if (!string.IsNullOrEmpty(str))
                        {
                            float n;
                            string[] parts = str.Split(',');
                            for (int i = 0; i < parts.Length; i++)
                            {
                                if (float.TryParse(parts[i], out n))
                                {
                                    switch (i)
                                    {
                                        case 0:
                                            v.x = n;
                                            break;
                                        case 1:
                                            v.y = n;
                                            break;
                                        case 2:
                                            v.width = n;
                                            break;
                                        case 3:
                                            v.height = n;
                                            break;
                                    }
                                }
                            }
                        }
                        value = v;
                    }
                    break;
                case SerializableTypeCode.RectInt:
                    {
                        RectInt v = new RectInt();
                        if (!string.IsNullOrEmpty(str))
                        {
                            int n;
                            string[] parts = str.Split(',');
                            for (int i = 0; i < parts.Length; i++)
                            {
                                if (int.TryParse(parts[i], out n))
                                {
                                    switch (i)
                                    {
                                        case 0:
                                            v.x = n;
                                            break;
                                        case 1:
                                            v.y = n;
                                            break;
                                        case 2:
                                            v.width = n;
                                            break;
                                        case 3:
                                            v.height = n;
                                            break;
                                    }
                                }
                            }
                        }
                        value = v;
                    }
                    break;
                case SerializableTypeCode.RectOffset:
                    {
                        RectOffset v;

                        int l = 0, r = 0, t = 0, b = 0;
                        if (!string.IsNullOrEmpty(str))
                        {
                            int n;
                            string[] parts = str.Split(',');
                            for (int i = 0; i < parts.Length; i++)
                            {
                                if (int.TryParse(parts[i], out n))
                                {
                                    switch (i)
                                    {
                                        case 0:
                                            l = n;
                                            break;
                                        case 1:
                                            r = n;
                                            break;
                                        case 2:
                                            t = n;
                                            break;
                                        case 3:
                                            b = n;
                                            break;
                                    }
                                }
                            }
                        }
                        v = new RectOffset(l, r, t, b);
                        value = v;
                    }
                    break;
                case SerializableTypeCode.RectOffsetSerializable:
                    {
                        SerializableRectOffset v;

                        int l = 0, r = 0, t = 0, b = 0;
                        if (!string.IsNullOrEmpty(str))
                        {
                            int n;
                            string[] parts = str.Split(',');
                            for (int i = 0; i < parts.Length; i++)
                            {
                                if (int.TryParse(parts[i], out n))
                                {
                                    switch (i)
                                    {
                                        case 0:
                                            l = n;
                                            break;
                                        case 1:
                                            r = n;
                                            break;
                                        case 2:
                                            t = n;
                                            break;
                                        case 3:
                                            b = n;
                                            break;
                                    }
                                }
                            }
                        }
                        v = new SerializableRectOffset(l, r, t, b);
                        value = v;
                    }
                    break;
                case SerializableTypeCode.AnimationCurve:
                    AnimationCurve curve;
                    if (!TryParseCurve(str, out curve))
                        curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
                    value = curve;
                    break;

            }

            return value;
        }

        static string SerializeString(float f)
        {
            return f.ToString("0.##");
        }

        static string ToCurveString(AnimationCurve curve)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            var keys = curve.keys;

            for (int i = 0, len = keys.Length; i < len; i++)
            {
                var key = keys[i];

                if (i > 0)
                    sb.Append(",");
                sb.Append('[')
                    .Append(SerializeString(key.time))
                    .Append(',')
                    .Append(SerializeString(key.value))
                    .Append(',')
                    .Append(SerializeString(key.inTangent))
                    .Append(',')
                    .Append(SerializeString(key.outTangent))
                    .Append(',')
                    .Append(SerializeString(key.inWeight))
                    .Append(',')
                    .Append(SerializeString(key.outWeight))
                    .Append(']');
            }
            sb.Append("]");
            return sb.ToString();
        }

        static AnimationCurve ParseCurve(string str)
        {
            AnimationCurve curve;

            if (str == null)
                throw new ArgumentNullException("str");

            str = str.Trim();

            if (str.Length > 0 && str[0] == '[')
                str = str.Substring(1);
            if (str.Length > 0 && str[str.Length - 1] == ']')
                str = str.Substring(0, str.Length - 1);

            float time, value, inTangent, outTangent, inWeight, outWeight;
            Keyframe keyframe;
            List<Keyframe> keys = new List<Keyframe>();
            foreach (var part in str.Split(']'))
            {
                string tmp = part.Trim();
                if (tmp.Length == 0)
                    continue;
                if (tmp[0] == ',')
                    tmp = tmp.Substring(2);
                else
                    tmp = tmp.Substring(1);

                string[] arr = tmp.Split(',');

                time = float.Parse(arr[0]);
                value = float.Parse(arr[1]);

                if (arr.Length > 2)
                    inTangent = float.Parse(arr[2]);
                else
                    inTangent = 0f;
                if (arr.Length > 3)
                    outTangent = float.Parse(arr[3]);
                else
                    outTangent = 0f;
                if (arr.Length > 4)
                    inWeight = float.Parse(arr[4]);
                else
                    inWeight = 0f;
                if (arr.Length > 5)
                    outWeight = float.Parse(arr[5]);
                else
                    outWeight = 0f;
                keyframe = new Keyframe(time, value, inTangent, outTangent, inWeight, outWeight);
                keys.Add(keyframe);
            }

            curve = new AnimationCurve(keys.ToArray());
            return curve;
        }

        static bool TryParseCurve(string str, out AnimationCurve curve)
        {
            if (string.IsNullOrEmpty(str))
            {
                curve = new AnimationCurve();
                return false;
            }

            try
            {
                curve = ParseCurve(str);
                return true;
            }
            catch
            {
                curve = new AnimationCurve();
                return false;
            }
        }



        public static implicit operator T(Serializable<T> a)
        {
            return a.Target;
        }

    }


}