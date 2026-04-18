using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Serialization;
using UnityEditor;
using UnityEditor.UIElements.Extension;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(SerializableObjectDroplistAttribute), true)]
public class SerializableObjectDroplistPropertyDrawer : PropertyDrawer
{
    static Dictionary<Type, List<Type>> cacheTypeList;
    static Dictionary<Type, List<GUIContent>> cacheDisplayList;
    //public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    // {
    //     var attr = attribute as SerializableObjectDroplistAttribute;
    //     var baseType = attr.BaseType;

    //     SerializableObject selectedValue = property.GetValue() as SerializableObject;
    //     /*
    //     selectedValue = GUIDroplist(position, label, selectedValue, attr, attr.Inputable);

    //     property.SetValue(selectedValue);
    //     */

    // }

    string TypeToDisplayString(Type type)
    {
        return type != null ? type.Name : "None";
    }

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var attr = attribute as SerializableObjectDroplistAttribute;
        var baseType = attr.BaseType;
        Type fieldType = fieldInfo.FieldType;

        if (baseType == null)
        {
            if (fieldType.IsGenericType)
            {
                baseType = fieldType.GetGenericArguments()[0];
            }
        }
        VisualElement root = new VisualElement();
        if (baseType == null)
        {
            Label errorLabel = new Label();
            errorLabel.text = $"({fieldInfo.DeclaringType}: {fieldInfo}) BaseType null";
            root.Add(errorLabel);
            return root;
        }
        if (cacheTypeList == null)
        {
            cacheTypeList = new();
            cacheDisplayList = new();
        }
        List<Type> typeList;
        List<GUIContent> displayList;
        if (!cacheTypeList.TryGetValue(baseType, out typeList))
        {
            typeList = new List<Type>();
            displayList = new List<GUIContent>();
            foreach (var type in TypeCache.GetTypesDerivedFrom(baseType).OrderBy(_ => _.Name))
            {
                if (type.IsAbstract) continue;
                typeList.Add(type);
                displayList.Add(new GUIContent(type.Name));
            }
            cacheTypeList[baseType] = typeList;
            cacheDisplayList[baseType] = displayList;
        }

        if (fieldType == typeof(SerializableObject))
        {

        }

        object serializableObject = property.boxedValue;
        if (serializableObject == null)
        {
            serializableObject = Activator.CreateInstance(fieldType);
            property.boxedValue = serializableObject;
        }

        VisualElement memberContainer = new VisualElement();

        DropdownField dropdownField = new DropdownField();
        root.Add(dropdownField);
        dropdownField.choices.Clear();
        dropdownField.choices.Add("None");
        foreach (var type in typeList)
        {
            dropdownField.choices.Add(TypeToDisplayString(type));
        }

        dropdownField.SetValueWithoutNotify(TypeToDisplayString(GetTarget(serializableObject)?.GetType()));

        dropdownField.RegisterValueChangedCallback(e =>
        {
            Type type;
            if (e.newValue == "None")
            {
                type = null;
            }
            else
            {
                type = typeList.FirstOrDefault(_ => _.Name == e.newValue);
            }

            serializableObject = property.boxedValue;
            if (type != GetTarget(serializableObject)?.GetType())
            {
                object target = null;
                if (type != null)
                {
                    target = Activator.CreateInstance(type);
                }
                SetTarget(serializableObject, target);
                property.boxedValue = serializableObject;
                property.serializedObject.ApplyModifiedProperties();
                CreateFields(property, memberContainer, serializableObject);
                //EditorUtility.SetDirty(property.serializedObject.targetObject);
            }
        });


        root.Add(memberContainer);
        CreateFields(property, memberContainer, serializableObject);
        return root;
    }


    void CreateFields(SerializedProperty property, VisualElement container, object serializableObject)
    {
        container.Clear();

        if (GetTarget(serializableObject) != null)
        {
            foreach (var item in EditorUIElementsUtility.CreateMemberInputFields(new CreateMemberInputOptions()
            {
                target = GetTarget(serializableObject),
                parent = container
            }))
            {
                /*  container.Add(item.inputField);
                  var inputField = item.inputField;
                  var field = item.fieldInfo;

                  if (!inputField.RegisterValueChangeCallback((e, newValue) =>
                  {
                      var value = selectedValue.Target;
                      field.SetValue(value, newValue);
                      selectedValue.Target = value;
                      property.boxedValue = selectedValue;
                      property.serializedObject.ApplyModifiedProperties();
                  }))
                  {

                  }    */

                item.InputView.ValueChanged += (newValue) =>
                {
                    property.boxedValue = serializableObject;
                    property.serializedObject.ApplyModifiedProperties();
                };
            }
        }
    }

    static object GetTarget(object serializableObject)
    {
        if (serializableObject == null) return null;
        if (serializableObject is SerializableObject so)
        {
            return so.Target;
        }
        PropertyInfo property = serializableObject.GetType().GetProperty("Target");
        return property.GetValue(serializableObject);
    }

    static void SetTarget(object serializableObject, object target)
    {
        if (serializableObject == null) throw new ArgumentNullException(nameof(serializableObject));
        if (serializableObject is SerializableObject so)
        {
            so.Target = target;
            return;
        }
        PropertyInfo property = serializableObject.GetType().GetProperty("Target");
        property.SetValue(serializableObject, target);
    }
    /*
    public static object GUIDroplist(Rect position, GUIContent label, SerializableObject selectedValue, bool showText = false)
    {
      

        return GUIDroplist(position, label, selectedValue, valueList.ToArray(), displayList.ToArray(), showText);
    }
    public static object GUIDroplist(Rect position, GUIContent label, SerializableObject selectedValue, object[] values, GUIContent[] display)
    {
        position = EditorGUI.PrefixLabel(position, label);
        position.xMin -= EditorGUI.indentLevel * 15;

        int newSelectedIndex;


        int selectedIndex = -1;

        for (int i = 0; i < values.Length; i++)
        {
            if (object.Equals(values[i], selectedValue.Value?.GetType()))
            {
                selectedIndex = i;
                break;
            }
        }
        newSelectedIndex = EditorGUI.Popup(position, selectedIndex, display);

        if (newSelectedIndex != selectedIndex)
        {
            selectedIndex = newSelectedIndex;
            if (selectedIndex != -1)
            {
                selectedValue = values[selectedIndex];
                GUI.changed = true;
            }
        }
        return selectedValue;
    }*/
}
/*
[CustomView(typeof(playerRan))]
public class SerializableObjectView
{

}*/