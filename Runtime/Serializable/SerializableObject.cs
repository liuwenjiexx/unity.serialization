using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Serialization
{
  
        [Serializable]
        struct SerializableObject3
        {
            /// <summary>
            /// type name
            /// </summary>
            [SerializeField]
            public string t;
            public SerializableTypeCode c;
            //public SerializableValue v;
            /// <summary>
            /// members
            /// </summary>
            [SerializeField]
            public List<SerializableMember> m;

            /// <summary>
            /// Reference member
            /// </summary>
            //[SerializeField]
            //public List<SerializableMember> r;


            public SerializableObject3(Type type, Type hintType = null)
            {
                t = null;
                c = SerializableTypeCode.Null;
                m = null;
                SetType(type, hintType);
            }



            private void SetType(Type type, Type hintType)
            {
                if (type == null)
                {
                    //c = SerializableValue.Null;
                    SetNull();
                    return;
                }

                var typeCode = SerializableUtility.TypeToSerializableTypeCode(type, out var itemType);
                c = typeCode;

                if (type != hintType)
                {
                    if (itemType != null)
                    {
                        var itemTypeCode = typeCode & (~SerializableTypeCode.Array);
                        if (type.IsArray)
                        {
                            if (typeCode == SerializableTypeCode.Object)
                            {
                                t = itemType.AssemblyQualifiedName;
                            }
                        }
                        else
                        {
                            t = type.AssemblyQualifiedName;
                        }
                    }
                    else if (typeCode == SerializableTypeCode.Object)
                    {
                        t = type.AssemblyQualifiedName;
                    }
                }
            }

            public bool FindMember(string name, out SerializableMember member)
            {
                if (m == null)
                {
                    member = default;
                    return false;
                }
                foreach (var memberValue in m)
                {
                    if (memberValue.n == name)
                    {
                        member = memberValue;
                        return true;
                    }
                }
                member = default;
                return false;
            }

            public void SetNull()
            {
                c = SerializableTypeCode.Null;

            }

        }
    } 
