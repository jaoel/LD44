using System.Reflection;
using System.Collections;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ProbabilityListBase), true)]
class ProbabilityListPropertyDrawer : PropertyDrawer
{

    private float _lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded) return EditorGUIUtility.singleLineHeight;
        SerializedProperty values = property.FindPropertyRelative("_values");
        ProbabilityListBase list = SerializedPropertyUtil.GetTargetObjectOfProperty(property) as ProbabilityListBase;
        System.Type listType = list.GetTemplateType();
        return (values.arraySize + 2 + (listType.IsSubclassOf(typeof(Object)) ? 2 : 0)) * _lineHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ProbabilityListBase list = SerializedPropertyUtil.GetTargetObjectOfProperty(property) as ProbabilityListBase;

        System.Type listType = list.GetTemplateType();

        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty values = property.FindPropertyRelative("_values");
        SerializedProperty priorities = property.FindPropertyRelative("_priorities");
        SerializedProperty totalPriority = property.FindPropertyRelative("_totalPriority");

        Rect elementRect = new Rect(position);
        elementRect.height = EditorGUIUtility.singleLineHeight;

        property.isExpanded = EditorGUI.Foldout(elementRect, property.isExpanded, new GUIContent(label.text + " (" + values.arraySize + ")", label.image, label.tooltip), true);
        elementRect.y += _lineHeight;

        if (property.isExpanded)
        {
            int origIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 1;
            elementRect = EditorGUI.IndentedRect(elementRect);
            EditorGUI.indentLevel = origIndent;

            if (listType.IsSubclassOf(typeof(Object)))
            {
                Rect dragDropRect = elementRect;
                dragDropRect.height *= 2.0f;
                EditorGUI.HelpBox(dragDropRect, "Drag & drop here to add items!", MessageType.Info);
                elementRect.y += 2.0f * _lineHeight;

                if (dragDropRect.Contains(Event.current.mousePosition))
                {
                    if (Event.current.type == EventType.DragUpdated)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                    }
                    else if (Event.current.type == EventType.DragPerform)
                    {
                        for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                        {
                            Object obj = DragAndDrop.objectReferences[i];
                            bool validInsert = false;
                            if (obj is GameObject)
                            {
                                GameObject go = obj as GameObject;
                                if (listType == typeof(GameObject) || (listType.IsSubclassOf(typeof(Component)) && go.GetComponent(listType) != null))
                                {
                                    validInsert = true;
                                }
                            }
                            else if (obj.GetType().IsAssignableFrom(listType))
                            {
                                validInsert = true;
                            }
                            if (validInsert)
                            {
                                int insert_index = values.arraySize;
                                values.InsertArrayElementAtIndex(insert_index);
                                priorities.InsertArrayElementAtIndex(insert_index);
                                values.GetArrayElementAtIndex(insert_index).objectReferenceValue = obj;
                                priorities.GetArrayElementAtIndex(insert_index).intValue = 1;
                            }
                        }
                        Event.current.Use();
                    }
                }
            }


            int total = 0;
            for (int i = 0; i < values.arraySize; i++)
            {
                SerializedProperty value = values.GetArrayElementAtIndex(i);
                SerializedProperty priority = priorities.GetArrayElementAtIndex(i);
                total += priority.intValue;

                Rect objectRect = elementRect;
                objectRect.width = Mathf.Floor(elementRect.width * 0.5f);
                if (listType.IsSubclassOf(typeof(Object)))
                {
                    value.objectReferenceValue = EditorGUI.ObjectField(objectRect, value.objectReferenceValue, listType, true);
                }
                else
                {
                    EditorGUI.PropertyField(objectRect, value, new GUIContent());
                }

                Rect priorityRect = elementRect;
                priorityRect.width = 60.0f;
                priorityRect.x += objectRect.width;
                priority.intValue = EditorGUI.IntField(priorityRect, priority.intValue);

                float percentage = totalPriority.intValue > 0 ? (float)priority.intValue / totalPriority.intValue : 0.0f;

                Rect percentageRect = elementRect;
                percentageRect.x += priorityRect.width + objectRect.width;
                percentageRect.width = 60.0f;
                EditorGUI.LabelField(percentageRect, string.Format("{0:P}", percentage));

                Rect deleteRect = elementRect;
                deleteRect.width = 60.0f;
                deleteRect.x += elementRect.width - deleteRect.width;
                if (GUI.Button(deleteRect, "Delete"))
                {
                    if (values.GetArrayElementAtIndex(i).propertyType == SerializedPropertyType.ObjectReference)
                    {
                        values.GetArrayElementAtIndex(i).objectReferenceValue = null;
                    }
                    values.DeleteArrayElementAtIndex(i);
                    priorities.DeleteArrayElementAtIndex(i);
                }

                elementRect.y += _lineHeight;
            }

            totalPriority.intValue = total;

            Rect addRect = elementRect;
            addRect.width = 60.0f;
            addRect.x += elementRect.width - addRect.width;
            if (GUI.Button(addRect, "Add"))
            {
                int insert_index = values.arraySize;
                values.InsertArrayElementAtIndex(insert_index);
                priorities.InsertArrayElementAtIndex(insert_index);
                priorities.GetArrayElementAtIndex(insert_index).intValue = 1;
            }
            elementRect.y += _lineHeight;
        }

        EditorGUI.EndProperty();
    }
}

public static class SerializedPropertyUtil
{
    public static object GetTargetObjectOfProperty(SerializedProperty prop)
    {
        string path = prop.propertyPath.Replace(".Array.data[", "[");
        object obj = prop.serializedObject.targetObject;
        string[] elements = path.Split('.');
        foreach (string element in elements)
        {
            if (element.Contains("["))
            {
                string elementName = element.Substring(0, element.IndexOf("["));
                int index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                obj = GetValue(obj, elementName, index);
            }
            else
            {
                obj = GetValue(obj, element);
            }
        }
        return obj;
    }

    private static object GetValue(object source, string name)
    {
        if (source == null)
        {
            return null;
        }

        System.Type type = source.GetType();

        while (type != null)
        {
            FieldInfo fieldInfo = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (fieldInfo != null)
                return fieldInfo.GetValue(source);

            PropertyInfo propertyInfo = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (propertyInfo != null)
                return propertyInfo.GetValue(source, null);

            type = type.BaseType;
        }
        return null;
    }

    private static object GetValue(object source, string name, int index)
    {
        IEnumerable enumerable = GetValue(source, name) as IEnumerable;
        if (enumerable == null)
        {
            return null;
        }

        IEnumerator enumerator = enumerable.GetEnumerator();

        for (int i = 0; i <= index; i++)
        {
            if (!enumerator.MoveNext()) return null;
        }
        return enumerator.Current;
    }
}
#endif
