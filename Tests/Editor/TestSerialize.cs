using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Serialization.EditorTests
{

    public class TestSerialize
    {
        [Test]
        public void Base_Type()
        {
            BaseType target = new BaseType()
            {
                str = "Hello World!",
                int32 = 1,
                int64 = long.MaxValue,
                float32 = 1.2f,
                float64 = double.MaxValue,
                enumValue = EnumValue.One,
                ignorePublicField = "AA",
            };

            var json = JsonUtility.ToJson(new Serializable<BaseType>(target), true);
            Debug.Log(json);

            Debug.Log("long max: " + long.MaxValue);
            Debug.Log("double max: " + double.MaxValue);
        }

        [Test]
        public void Unity_Type()
        {
            UnityType target = new UnityType()
            {
                v2 = new Vector2(1, 2),
                v3 = new Vector3(1, 2, 3),
                rect = new Rect(1, 2, 3, 4),
                offset = new RectOffset(1, 2, 3, 4),
            };

            var json = JsonUtility.ToJson(new Serializable<UnityType>(target), true);
            Debug.Log(json);
        }

        [Test]
        public void Array_Int32()
        {
            string[] stringArray = new string[] { "abc", "123" };
            var json = JsonUtility.ToJson(new Serializable<string[]>(stringArray), true);
            Debug.Log(json);
        }
        [Test]
        public void Array_String()
        {
            string[] stringArray = new string[] { "abc", "123" };
            var json = JsonUtility.ToJson(new Serializable<string[]>(stringArray), true);
            Debug.Log(json);
        }
        [Test]
        public void Array_Class()
        {
            BaseType[] array = new BaseType[] {
            new BaseType() { str = "abc", int32 = 123 },
            new BaseType() { str = "def", int32 = 456 }
        };
            var json = JsonUtility.ToJson(new Serializable<BaseType[]>(array), true);
            Debug.Log(json);
        }

        [Test]
        public void Sub_Array()
        {
            ArrayMemberType subArray = new ArrayMemberType();
            subArray.array = new BaseType[] {
            new BaseType() { str = "abc", int32 = 123 },
            new BaseType() { str = "def", int32 = 456 }
        };
            subArray.list = new List<BaseType>(){
            new BaseType() { str = "a", int32 = 1 },
            new BaseType() { str = "b", int32 = 2 }
        };

            var json = JsonUtility.ToJson( new Serializable<ArrayMemberType>(subArray), true);
            Debug.Log(json);
        }


        [Test]
        public void Property()
        {
            PropertyClass target = new PropertyClass()
            {
                Int32 = 1,
                Str = "abc"
            };

            var json = JsonUtility.ToJson(new Serializable<PropertyClass>(target, SerializableOptions.Property), true);
            Debug.Log(json);
        }
        [Test]
        public void Dictionary()
        {
            SerializableDictionary<int, string> dic = new SerializableDictionary<int, string>();
            dic[0] = "Hello";
            dic[1] = "World";
            var json= JsonUtility.ToJson(dic, true);
            Debug.Log(json);
            dic= JsonUtility.FromJson<SerializableDictionary<int,string>>(json);
            Assert.AreEqual(2, dic.Count);
            Assert.AreEqual("Hello", dic[0]);
            Assert.AreEqual("World", dic[1]);
        }

    }
   
 
}