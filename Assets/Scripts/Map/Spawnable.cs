using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpawnableContainer))]
public class SpawnableInspector : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("Spawnable keyframe data"); 
    }
}

[CreateAssetMenu]
[Serializable]
public class SpawnableContainer : ScriptableObject
{
    List<SpawnableKeyframe> keyframes;
}


[Serializable]
public class SpawnableKeyframe
{
    public int keyframeIndex;
    public List<Spawnable> spawnableObjects;
}


[Serializable]
public class Spawnable
{
    public GameObject spawnablePrefab;
    public List<float> densities;
}
