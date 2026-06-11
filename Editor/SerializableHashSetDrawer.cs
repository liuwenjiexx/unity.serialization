
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Serialization.Editor
{

    [CustomPropertyDrawer(typeof(ISerializableHashSet), true)]
    public class SerializableHashSetDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new VisualElement();
            root.style.flexDirection = FlexDirection.Row;
            Label label = new Label();
            label.text = this.preferredLabel;
            label.style.width = EditorGUIUtility.labelWidth;
            root.Add(label);
            VisualElement valueContainer = new VisualElement();
            root.Add(valueContainer);
            valueContainer.style.flexGrow = 1f;
           //StyleSheet styleSheet= new StyleSheet();
    
           // root.styleSheets.Add(styleSheet);

            SerializedProperty itemsProperty = property.FindPropertyRelative("items");

            Action Refresh = () =>
            {
                valueContainer.Clear();

                for (int i = valueContainer.childCount; i < itemsProperty.arraySize; i++)
                {
                    var itemProperty = itemsProperty.GetArrayElementAtIndex(i);
                    var propertyField = new PropertyField(itemProperty);
                    propertyField.style.minHeight = EditorGUIUtility.singleLineHeight;
                    propertyField.label = null;
                    valueContainer.Add(propertyField);

                    //Label a = new Label();
                    //a.text = i.ToString()+", "+propertyField.bindingPath+", "+itemProperty.propertyPath;
                    //valueContainer.Add(a);
                }

                for (int i = valueContainer.childCount; i > itemsProperty.arraySize; i--)
                {
                    valueContainer.RemoveAt(i - 1);
                }

                //修复值改变后不显示
                foreach (var i in valueContainer.Query<PropertyField>().Build())
                {
                    i.Bind(property.serializedObject);
                    i.label = null;
                    var tmpLabel = i.Q<Label>();
                    if (tmpLabel != null)
                        tmpLabel.style.display = DisplayStyle.None;
                }
            };
            Refresh();
            root.TrackPropertyValue(property, (prop) =>
            {
                //property = prop;
                //Debug.Log("TrackPropertyValue " + prop.name);
                Refresh();
            });
            return root;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty itemsProperty = property.FindPropertyRelative("items");
            return itemsProperty.arraySize * EditorGUIUtility.singleLineHeight;
        }
    }
}