using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour {
    [SerializeField] private Camera mainCamera;
    public Camera MainCamera => mainCamera;

    private static CameraManager instance = null;
    public static CameraManager Instance {
        get {
            if (instance != null) {
                return instance;
            }
            instance = FindObjectOfType<CameraManager>();
            if (instance == null || instance.Equals(null)) {
                Debug.LogError("The scene needs a CameraManager");
            }
            return instance;
        }
    }

    void Start() {
        if(mainCamera == null) {
            mainCamera = FindObjectOfType<Camera>();
        }
    }
}
