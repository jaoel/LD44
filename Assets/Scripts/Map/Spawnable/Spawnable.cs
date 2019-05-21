using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Spawnable
{
    [SerializeField]
    public GameObject spawnablePrefab;

    [SerializeField]
    public float density;
}
