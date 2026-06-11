using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity;
using UnityEngine;


namespace Unity.Serialization.EditorTests
{

    public class TestDeserialize
    {

        BaseType BaseTypeData()
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
            return target;
        }
        BaseType[] BaseTypeArrayData()
        {
            return new BaseType[]{
            new BaseType()
        {
            str = "Hello World!",
            int32 = 1,
            enumValue = EnumValue.One,
            ignorePublicField = "AA",
        },
            new BaseType()
        {
            str = "123",
            int32 = 2,
            enumValue = EnumValue.Two,
        } };
        }

        List<BaseType> BaseTypeListData()
        {
            return new List<BaseType>{
            new BaseType()
        {
            str = "Hello World!",
            int32 = 1,
            enumValue = EnumValue.One,
            ignorePublicField = "AA",
        },
            new BaseType()
        {
            str = "123",
            int32 = 2,
            enumValue = EnumValue.Two,
        } };
        }



        [Test]
        public void Base_Type()
        {
            var target = BaseTypeData();

            var json = JsonUtility.ToJson(new Serializable<BaseType>(target), true);

            var deserialize = JsonUtility.FromJson<Serializable<BaseType>>(json);
            Assert.IsNotNull(deserialize);

            var target2 = deserialize.Target;
            Assert.IsNotNull(target2);

            Assert.AreEqual(target.str, target2.str);
            Assert.AreEqual(target.int32, target2.int32);
            Assert.AreEqual(target.enumValue, target2.enumValue);
            Assert.AreEqual(null, target2.ignorePublicField);
            Assert.AreEqual(long.MaxValue, target2.int64);
            Assert.AreEqual(double.MaxValue, target2.float64);
        }



        [Test]
        public void Array_BaseType()
        {

            var array = BaseTypeArrayData();

            var json = JsonUtility.ToJson(new Serializable<BaseType[]>(array), true);

            var deserialize = JsonUtility.FromJson<Serializable<BaseType[]>>(json);
            Assert.IsNotNull(deserialize);

            var array2 = deserialize.Target;
            Assert.IsNotNull(array2);
            Assert.AreEqual(array.Length, array2.Length);

            for (int i = 0; i < array.Length; i++)
            {
                var target = array[i];
                var target2 = array2[i];
                Assert.AreEqual(target.str, target2.str);
                Assert.AreEqual(target.int32, target2.int32);
                Assert.AreEqual(target.enumValue, target2.enumValue);
            }
        }

        [Test]
        public void List_BaseType()
        {

            var array = BaseTypeListData();

            var json = JsonUtility.ToJson(new Serializable<List<BaseType>>(array), true);

            var deserialize = JsonUtility.FromJson<Serializable<List<BaseType>>>(json);
            Assert.IsNotNull(deserialize);

            var array2 = deserialize.Target;
            Assert.IsNotNull(array2);
            Assert.AreEqual(array.Count, array2.Count);

            for (int i = 0; i < array.Count; i++)
            {
                var target = array[i];
                var target2 = array2[i];
                Assert.AreEqual(target.str, target2.str);
                Assert.AreEqual(target.int32, target2.int32);
                Assert.AreEqual(target.enumValue, target2.enumValue);
            }
        }

        [Test]
        public void Sub_Array()
        {
            ArrayMemberType subArray = new ArrayMemberType();
            subArray.array = new BaseType[] {
            new BaseType() { str = "abc", int32 = 123,enumValue= EnumValue.One },
            new BaseType() { str = "def", int32 = 456,  enumValue= EnumValue.Two }
        };
            subArray.list = new List<BaseType>(){
            new BaseType() { str = "a", int32 = 1 ,enumValue= EnumValue.One},
            new BaseType() { str = "b", int32 = 2, enumValue=EnumValue.Two }
        };

            var json = JsonUtility.ToJson(new Serializable<ArrayMemberType>(subArray), true);
            Debug.Log(json);


            var deserialize = JsonUtility.FromJson<Serializable<ArrayMemberType>>(json);
            Assert.IsNotNull(deserialize);

            var subArray2 = deserialize.Target;
            Assert.IsNotNull(subArray2);
            Assert.AreEqual(subArray.array.Length, subArray2.array.Length);
            Assert.AreEqual(subArray.list.Count, subArray2.list.Count);

            for (int i = 0; i < subArray.array.Length; i++)
            {
                var target = subArray.array[i];
                var target2 = subArray2.array[i];
                Assert.AreEqual(target.str, target2.str);
                Assert.AreEqual(target.int32, target2.int32);
   
                Assert.AreEqual(target.enumValue, target2.enumValue);
            }

            for (int i = 0; i < subArray.list.Count; i++)
            {
                var target = subArray.list[i];
                var target2 = subArray2.list[i];
                Assert.AreEqual(target.str, target2.str);
                Assert.AreEqual(target.int32, target2.int32);
                Assert.AreEqual(target.enumValue, target2.enumValue);
            }

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

            var deserialize = JsonUtility.FromJson<Serializable<PropertyClass>>(json);
            Assert.IsNotNull(deserialize);

            var target2 = deserialize.Target;
            Assert.IsNotNull(target2);

            Assert.AreEqual(target.Int32, target2.Int32);
            Assert.AreEqual(target.Str, target2.Str);

        }
    }

}