using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class NavigationManager : MonoBehaviour
{
    private static NavigationManager instance;
    public static NavigationManager Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }
            instance = FindObjectOfType<NavigationManager>();
            if (instance == null || instance.Equals(null))
            {
                Debug.LogError("The scene needs a NavigationManager");
            }
            return instance;
        }
    }
}
