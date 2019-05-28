using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public PlayerUI playerUI;

    private static UIManager _instance = null;
    public static UIManager Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }

            _instance = FindObjectOfType<UIManager>();

            if (_instance == null || _instance.Equals(null))
            {
                Debug.LogError("The scene needs a UIManager");
            }
            return _instance;
        }
    }

    private void Start()
    {
        if (playerUI == null)
        {
            playerUI = FindObjectOfType<PlayerUI>();
        }
    }
}
