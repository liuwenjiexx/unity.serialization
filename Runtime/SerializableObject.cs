using System;
using UnityEngine;

namespace Unity.Serialization
{ 
    
    // 非泛型适合反射使用 
    [Serializable]
    public class SerializableObject : ISerializationCallbackReceiver
    {
        [NonSerialized]
        protected object target;
        [SerializeField]
        private string typeName;
        [SerializeField]
        private string data;
        [NonSerialized]
        protected Exception deserializeError;
        //[SerializeField]
        //private bool delay;


        /// <summary>
        /// System.Type
        /// </summary>
        const string TYPE_NAME_TYPE = ":T";
        const string TYPE_NAME_TYPE2 = "Type";

        public SerializableObject()
        {
            data = null;
            typeName = null;
            deserializeError = null;
        }

        public SerializableObject(object target)
            : this()
        {
            this.Target = target;
        }


        public virtual object Target
        {
            get
            {
                return target;
            }

            set
            {
                if (target != value)
                {
                    target = value;
                    typeName = null;
                    data = null;
                    deserializeError = null;
                }
            }
        }

        //public bool IsDelayDeserialize
        //{
        //    get => delay;
        //    set => delay = value;
        //}

        public Exception DeserializeError
        {
            get => deserializeError;
        }

        public virtual void OnBeforeSerialize()
        {
            if (deserializeError != null)
                return;
            //typeName = null;
            //data = null;

            if (target != null)
            {
                if (target.GetType() == typeof(Type))
                {
                    typeName = TYPE_NAME_TYPE2;
                    data = ((Type)(object)target).AssemblyQualifiedName;
                }
                else
                {
                    typeName = target.GetType().AssemblyQualifiedName;
                    data = JsonUtility.ToJson(target);
                }
            }
        }

        public virtual void OnAfterDeserialize()
        {
            try
            {

                deserializeError = null;
                target = null;

                if (!string.IsNullOrEmpty(typeName))
                {
                    Type type;

                    if (typeName == TYPE_NAME_TYPE || typeName == TYPE_NAME_TYPE2)
                    {
                        type = typeof(Type);
                    }
                    else
                    {
                        type = Type.GetType(typeName);
                    }

                    if (type != null)
                    {
                        if (type == typeof(Type))
                        {
                            if (!string.IsNullOrEmpty(data))
                            {
                                target = (object)Type.GetType(data);
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(data))
                            {
                                target = JsonUtility.FromJson(data, type);
                            }
                            else
                            {
                                if (type.IsValueType)
                                {
                                    target = Activator.CreateInstance(type);
                                }
                                else
                                {
                                    target = null;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                deserializeError = ex;
                Debug.LogException(ex);
            }
        }



    }

    //JsonUtility.ToJson(struct<T>)  Serialize fail
    /// <summary>
    /// 对象序列化
    /// 新的 <see cref="MonoBehaviour"/> 和 <see cref="ScriptableObject"/> 对象成员序列化使用 <see cref="SerializeReference"/>
    /// 简单对象序列化 json 使用该类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class SerializableObject<T> : SerializableObject
    {
        /* [NonSerialized]
         private T target;
         [SerializeField]
         private string typeName;
         [SerializeField]
         private string data;
         [NonSerialized]
         private Exception deserializeError;

         /// <summary>
         /// System.Type
         /// </summary>
         const string TYPE_NAME_TYPE = ":T";
         const string TYPE_NAME_TYPE2 = "Type";
          */
        public SerializableObject()
        {
        }

        public SerializableObject(T target)
            : base(target)
        {
            //this.target = target;
            //data = null;
            //typeName = null;
            //deserializeError = null; 
        }


        public virtual new T Target
        {
            get
            {
                return (T)base.Target;
            }

            set
            {
                //target = value;
                //deserializeError = null;
                base.Target = value;
            }
        }
        /*
           public Exception DeserializeError
           {
               get => deserializeError;
           }

           public void OnBeforeSerialize()
           {
               if (deserializeError != null)
                   return;
               typeName = null;
               data = null;

               if (target != null)
               {
                   if (typeof(T) == typeof(Type))
                   {
                       typeName = TYPE_NAME_TYPE2;
                       data = ((Type)(object)target).AssemblyQualifiedName;
                   }
                   else
                   {
                       typeName = target.GetType().AssemblyQualifiedName;
                       data = JsonUtility.ToJson(target);
                   }
               }
           }

           public void OnAfterDeserialize()
           { 
               try
               { 
                   deserializeError = null;
                   target = default;

                   if (!string.IsNullOrEmpty(typeName))
                   {
                       Type type;

                       if (typeName == TYPE_NAME_TYPE || typeName == TYPE_NAME_TYPE2)
                       {
                           type = typeof(Type);
                       }
                       else
                       {
                           type = Type.GetType(typeName);
                       }

                       if (type != null)
                       {
                           if (type == typeof(Type))
                           {
                               if (!string.IsNullOrEmpty(data))
                               {
                                   target = (T)(object)Type.GetType(data);
                               }
                           }
                           else
                           {
                               if (!string.IsNullOrEmpty(data))
                               {
                                   target = (T)JsonUtility.FromJson(data, type);
                               }
                               else
                               {
                                   target = default(T);
                               }
                           }
                       }
                   }
               }
               catch (Exception ex)
               {
                   deserializeError = ex;
               }
           }
        */

        public static implicit operator T(SerializableObject<T> a)
        {
            return a.Target;
        }
        public static implicit operator SerializableObject<T>(T a)
        {
            return new SerializableObject<T>(a);
        }
    }


   

    [Serializable]
    public class LazySerializableObject : SerializableObject
    {
        [NonSerialized]
        private bool hasValue;

        public LazySerializableObject()
        {
            hasValue = false;
        }

        public override object Target
        {
            get
            {
                if (!hasValue)
                {
                    hasValue = true;
                    base.OnAfterDeserialize();
                    if (deserializeError != null)
                        return default;
                }
                return base.Target;
            }
            set
            {
                hasValue = true;
                base.Target = value;
            }
        }

        public override void OnBeforeSerialize()
        {
            if (hasValue)
            {
                base.OnBeforeSerialize();
            }
        }

        public override void OnAfterDeserialize()
        {
        }
    }

    [Serializable]
    public class LazySerializableObject<T> : SerializableObject<T>
    {
        [NonSerialized]
        private bool hasValue;

        public LazySerializableObject()
        {
            hasValue = false;
        }

        public override T Target
        {
            get
            {
                if (!hasValue)
                {
                    hasValue = true;
                    base.OnAfterDeserialize();
                    if (deserializeError != null)
                        return default;
                }
                return base.Target;
            }
            set
            {
                hasValue = true;
                base.Target = value;
            }
        }

        public override void OnBeforeSerialize()
        {
            if (hasValue)
            {
                base.OnBeforeSerialize();
            }
        }

        public override void OnAfterDeserialize()
        {
        }
    }


}
