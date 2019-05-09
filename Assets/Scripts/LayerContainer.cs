using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class LayerContainer : MonoBehaviour
{
    public Dictionary<string, int> Layers;

    private static LayerContainer _instance = null;
    public static LayerContainer Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }

            _instance = FindObjectOfType<LayerContainer>();

            if (_instance == null || _instance.Equals(null))
            {
                Debug.LogError("The scene needs a BulletManager");
            }
            return _instance;
        }
    }

    public void Awake()
    {
        Layers = new Dictionary<string, int>();
        Layers.Add("Map", LayerMask.NameToLayer("Map"));
        Layers.Add("Player", LayerMask.NameToLayer("Player"));
        Layers.Add("FriendlyBullet", LayerMask.NameToLayer("FriendlyBullet"));
        Layers.Add("Enemy", LayerMask.NameToLayer("Enemy")); 
    }

    public static int CombinedLayerMask(params string[] layerNames)
    {
        int result = 0;
        for (int i = 0; i < layerNames.Length; i++)
        {
            result |= 1 << Instance.Layers[layerNames[i]];
        }

        return result;
    }
}
