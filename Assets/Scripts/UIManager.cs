using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour {
    public PlayerUI playerUI;

    private static UIManager instance = null;
    public static UIManager Instance {
        get {
            if (instance != null) {
                return instance;
            }
            instance = FindObjectOfType<UIManager>();
            if (instance == null || instance.Equals(null)) {
                Debug.LogError("The scene needs a UIManager");
            }
            return instance;
        }
    }

    private void Start() {
        if (playerUI == null) {
            playerUI = FindObjectOfType<PlayerUI>();
        }
    }
}
