using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class SpawnableKeyframe
{
    [SerializeField]
    public int keyframeIndex;

    [SerializeField]
    public List<Spawnable> spawnableObjects = new List<Spawnable>();
}