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

    public void SetCameraPosition(Vector2 position) {
        Vector3 oldPos = mainCamera.transform.position;
        mainCamera.transform.position = new Vector3(position.x, position.y, oldPos.z);
    }

    void Start() {
        if(mainCamera == null) {
            mainCamera = FindObjectOfType<Camera>();
        }
    }
}
