using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
[Serializable]
public class SpawnableContainer : ScriptableObject
{
    [SerializeField]
    public List<SpawnableKeyframe> keyframes = new List<SpawnableKeyframe>();
}
