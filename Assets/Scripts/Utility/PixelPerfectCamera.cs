using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class PixelPerfectCamera : MonoBehaviour
{
    public static int pixelScale = 3;
    public static int pixelsPerUnit = 16;

    private Camera _camera;

    void Awake()
    {
        _camera = GetComponent<Camera>();
        QualitySettings.vSyncCount = 1;
    }

    void LateUpdate()
    {
        float verticalPixels = Screen.height / pixelScale;
        float scaleFactor = verticalPixels / pixelsPerUnit / 2f;
        _camera.orthographicSize = scaleFactor;
    }
}
