using UnityEngine;
using System.Collections;
using DG.Tweening;

public class CameraManager : MonoBehaviour {
    [SerializeField] private Camera mainCamera;
    public Camera MainCamera => mainCamera;

    private GameObject cameraContainer;

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
        if (cameraContainer != null)
        {
            Vector3 oldPos = cameraContainer.transform.position;
            cameraContainer.transform.position = new Vector3(position.x, position.y, oldPos.z);
        }
    }

    public void ShakeCamera(float duration, float positionStrength, float rotationStrength)
    {
        Sequence shake = DOTween.Sequence();
        shake.Append(mainCamera.DOShakePosition(duration, positionStrength, 10, 90, false));
        shake.Join(mainCamera.DOShakeRotation(duration, rotationStrength, 10, 90, false));
        shake.Append(mainCamera.transform.DOLocalMove(Vector3.zero, 0.5f, true));
        shake.Append(mainCamera.transform.DORotate(Vector3.zero, 0.5f));
        shake.Play();
    }

    void Awake() {
        if (cameraContainer == null)
        {
            cameraContainer = GameObject.Find("CameraContainer");
        }

        if(mainCamera == null) {
            mainCamera = FindObjectOfType<Camera>();
        }
    }
}
