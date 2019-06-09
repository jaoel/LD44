using UnityEngine;
using System.Collections;
using DG.Tweening;

public class CameraManager : MonoBehaviour
{
    [SerializeField]
    private Camera _mainCamera;
    public Camera MainCamera => _mainCamera;

    private GameObject _cameraContainer;

    private static CameraManager _instance = null;
    public static CameraManager Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }

            _instance = FindObjectOfType<CameraManager>();

            if (_instance == null || _instance.Equals(null))
            {
                Debug.LogError("The scene needs a CameraManager");
            }
            return _instance;
        }
    }

    public void SetCameraPosition(Vector2 position)
    {
        if (_cameraContainer != null)
        {
            Vector3 oldPos = _cameraContainer.transform.position;
            _cameraContainer.transform.position = new Vector3(position.x, position.y, oldPos.z);
        }
    }

    public void ShakeCamera(float duration, float positionStrength, float rotationStrength,
        int positionVibrato = 10, float positionRandomness = 90.0f)
    {
        Sequence shake = DOTween.Sequence();
        shake.Append(_mainCamera.DOShakePosition(duration, positionStrength * SettingsManager.Instance.ScreenShakeScale,
            positionVibrato, positionRandomness, false));
        shake.Join(_mainCamera.DOShakeRotation(duration, rotationStrength * SettingsManager.Instance.ScreenShakeScale,
            10, 90, false));
        shake.Append(_mainCamera.transform.DOLocalMove(Vector3.zero, 0.5f, true));
        shake.Append(_mainCamera.transform.DORotate(Vector3.zero, 0.5f));
        shake.Play();
    }

    void Awake()
    {
        if (_cameraContainer == null)
        {
            _cameraContainer = GameObject.Find("CameraContainer");
        }

        if (_mainCamera == null)
        {
            _mainCamera = FindObjectOfType<Camera>();
        }
    }
}
