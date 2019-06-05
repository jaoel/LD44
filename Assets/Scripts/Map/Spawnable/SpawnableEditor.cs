using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
[CustomEditor(typeof(SpawnableContainer))]
public class SpawnableInspector : Editor
{
    public static SpawnableContainer targetContainer;
    private SerializedProperty _keyframes;
    private SerializedProperty _droppableItems;
    private float _lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

    private void OnEnable()
    {
        _keyframes = serializedObject.FindProperty("keyframes");
        _droppableItems = serializedObject.FindProperty("drops");
    }

    public static void DrawUILine(Color color, int thickness = 2, int padding = 10)
    {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
        r.height = thickness;
        r.y += padding / 2;
        r.x -= 2;
        r.width += 6;
        EditorGUI.DrawRect(r, color);
    }

    public override void OnInspectorGUI()
    {
        targetContainer = (SpawnableContainer)target;
        targetContainer.keyframes = targetContainer.keyframes.OrderBy(x => x.keyframeIndex).ToList();

        foreach(SpawnableKeyframe keyframe in targetContainer.keyframes)
        {
            foreach (SpawnableKeyframe other in targetContainer.keyframes)
            {
                other.spawnableObjects.ForEach(x =>
                {
                    if (x.spawnablePrefab != null)
                    {
                        if (!keyframe.spawnableObjects.Any(y => y.spawnablePrefab == x.spawnablePrefab))
                        {
                            keyframe.spawnableObjects.Add(new Spawnable());
                            keyframe.spawnableObjects.Last().spawnablePrefab = x.spawnablePrefab;
                        }
                    }
                });
            }
        }

        //serializedObject.Update();
        EditorGUILayout.LabelField("Droppable Item data");
        EditorGUILayout.PropertyField(_droppableItems, true);

        EditorGUILayout.Space();
        DrawUILine(Color.gray);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Spawnable keyframe data");
        EditorGUILayout.LabelField("Number of keyframes: " + _keyframes.arraySize);

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        for (int i = 0; i < _keyframes.arraySize; i++)
        {
            SerializedProperty keyframe = _keyframes.GetArrayElementAtIndex(i);

            float height = _lineHeight;

            if (keyframe.isExpanded)
            {
                height += _lineHeight * 4;
                SerializedProperty spawnableList = keyframe.FindPropertyRelative("spawnableObjects");
                height += spawnableList.arraySize * _lineHeight;
            }

            GUILayoutOption rowHeight = GUILayout.Height(height);
            EditorGUILayout.PropertyField(keyframe, rowHeight);
            
            if (keyframe.isExpanded)
            {
                if (GUILayout.Button("Delete keyframe"))
                {
                    _keyframes.DeleteArrayElementAtIndex(i);
                }
            }

            EditorGUILayout.Space();
            DrawUILine(Color.gray);
        }

        if(GUILayout.Button("Add keyframe"))
        {
            SpawnableContainer container = (SpawnableContainer)target;
            SpawnableKeyframe newKeyframe = new SpawnableKeyframe();

            if (container.keyframes.Count > 0)
            {
                newKeyframe.keyframeIndex = container.keyframes.Last().keyframeIndex + 1;
                newKeyframe.spawnableObjects = new List<Spawnable>();
                container.keyframes.Last().spawnableObjects.ForEach(x =>
                {
                    Spawnable newSpawnable = new Spawnable();
                    newSpawnable.spawnablePrefab = x.spawnablePrefab;
                    newSpawnable.density = x.density;
                    newKeyframe.spawnableObjects.Add(newSpawnable);
                });
            }

            container.keyframes.Add(newKeyframe);
        }

        serializedObject.ApplyModifiedProperties();
    }
}

[CustomPropertyDrawer(typeof(SpawnableKeyframe))]
public class SpawnableKeyframeDrawer : PropertyDrawer
{
    private float _lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty keyframeIndex = property.FindPropertyRelative("keyframeIndex");
        Rect elementRect = new Rect(position);
        elementRect.height = EditorGUIUtility.singleLineHeight;
        property.isExpanded = EditorGUI.Foldout(elementRect, property.isExpanded, new GUIContent("Keyframe #" + keyframeIndex.intValue, label.image, label.tooltip), true);
        elementRect.y += _lineHeight;

        if (property.isExpanded)
        {
            elementRect = EditorGUI.IndentedRect(elementRect);
            Rect keyframeInputRect = EditorGUI.PrefixLabel(elementRect, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("Keyframe: "));
            keyframeIndex.intValue = EditorGUI.IntField(keyframeInputRect, keyframeIndex.intValue);

            float width = elementRect.width;

            elementRect = EditorGUI.IndentedRect(elementRect);
            elementRect.y += _lineHeight * 1.2f;
            elementRect.width = width / 2;
            
            SerializedProperty spawnables = property.FindPropertyRelative("spawnableObjects");
            if (spawnables.arraySize > 0)
            {
                elementRect = EditorGUI.IndentedRect(elementRect);
                EditorGUI.LabelField(elementRect, "Spawnable object");
            
                Rect densityRect = new Rect(elementRect);
                densityRect.x += width / 2;
                densityRect = EditorGUI.IndentedRect(densityRect);
                EditorGUI.LabelField(densityRect, "Density");
            
                for (int i = 0; i < spawnables.arraySize; i++)
                {
                    SerializedProperty spawnable = spawnables.GetArrayElementAtIndex(i);
                    SerializedProperty density = spawnable.FindPropertyRelative("density");
                    SerializedProperty spawnablePrefab = spawnable.FindPropertyRelative("spawnablePrefab");
            
                    elementRect.y += _lineHeight;
                    elementRect.width = width / 2;
                    elementRect = EditorGUI.IndentedRect(elementRect);
                    EditorGUI.PropertyField(elementRect, spawnablePrefab, GUIContent.none);
            
                    densityRect = new Rect(elementRect);
                    densityRect.x += densityRect.width;
                    densityRect.width /= 2.0f;
                    densityRect = EditorGUI.IndentedRect(densityRect);
                    float newDensity = EditorGUI.FloatField(densityRect, density.floatValue);

                    if (newDensity != density.floatValue)
                    {
                        density.floatValue = newDensity;
                        spawnable.serializedObject.ApplyModifiedProperties();
                    }

                    densityRect.x += densityRect.width;
                    if (GUI.Button(densityRect, "Del"))
                    {
                        spawnables.DeleteArrayElementAtIndex(i);
                        SpawnableInspector.targetContainer.keyframes.ForEach(x =>
                        {
                            x.spawnableObjects.RemoveAt(i);
                        });

                        property.serializedObject.Update();
                    }
                }
            }

            elementRect.width = width;
            elementRect.y += _lineHeight * 1.5f;
            elementRect = EditorGUI.IndentedRect(elementRect);
            if (GUI.Button(elementRect, "Add spawnable object"))
            {
                spawnables.InsertArrayElementAtIndex(spawnables.arraySize);
            }
        }
        
        property.serializedObject.ApplyModifiedProperties();
        EditorGUI.EndProperty();
    }
}

#endif