﻿using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpawnableContainer : MonoBehaviour
{
    [SerializeField]
    public List<SpawnableKeyframe> keyframes = new List<SpawnableKeyframe>();
}
