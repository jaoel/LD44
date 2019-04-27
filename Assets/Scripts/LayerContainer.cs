﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class LayerContainer : MonoBehaviour
{
    public Dictionary<string, int> Layers;

    private static LayerContainer instance = null;
    public static LayerContainer Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }
            instance = FindObjectOfType<LayerContainer>();
            if (instance == null || instance.Equals(null))
            {
                Debug.LogError("The scene needs a BulletManager");
            }
            return instance;
        }
    }

    public void Awake()
    {
        Layers = new Dictionary<string, int>();
        Layers.Add("Map", LayerMask.NameToLayer("Map"));
        Layers.Add("Player", LayerMask.NameToLayer("Player"));
        Layers.Add("FriendlyBullet", LayerMask.NameToLayer("FriendlyBullet")); 
    }
}