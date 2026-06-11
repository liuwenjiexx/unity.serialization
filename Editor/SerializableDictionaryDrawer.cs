using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Serialization.Editor
{

    [CustomPropertyDrawer(typeof(ISerializableDictionary), true)]
    public class SerializableDictionaryDrawer : PropertyDrawer
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


            SerializedProperty keysProperty = property.FindPropertyRelative("keys");
            SerializedProperty valuesProperty = property.FindPropertyRelative("values");

            Action Refresh = () =>
            {
                valueContainer.Clear();

                for (int i = valueContainer.childCount; i < keysProperty.arraySize; i++)
                {
                    VisualElement valueItem = new VisualElement();
                    valueItem.style.flexDirection = FlexDirection.Row;
                    valueItem.style.flexGrow = 1f;

                    var keyProperty = keysProperty.GetArrayElementAtIndex(i);
                    var valueProperty = valuesProperty.GetArrayElementAtIndex(i);

                    var keyField = new PropertyField(keyProperty);
                    Label tmpLabel = keyField.Q<Label>();
                    if (tmpLabel != null)
                        tmpLabel.style.display = DisplayStyle.None;

                    valueItem.Add(keyField);
                    var valueField = new PropertyField(valueProperty);
                    tmpLabel = valueField.Q<Label>();
                    if (tmpLabel != null)
                        tmpLabel.style.display = DisplayStyle.None;
                    valueField.style.minHeight = EditorGUIUtility.singleLineHeight;
                    valueField.style.flexGrow = 1f;
                    valueItem.Add(valueField);

                    valueContainer.Add(valueItem);
                }

                for (int i = valueContainer.childCount; i > keysProperty.arraySize; i--)
                {
                    valueContainer.RemoveAt(i - 1);
                }
                foreach (var i in valueContainer.Query<PropertyField>().Build())
                {
                    i.Bind(property.serializedObject);
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