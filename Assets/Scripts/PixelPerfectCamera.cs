using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class PixelPerfectCamera : MonoBehaviour {
    public static int pixelScale = 3;
    public static int pixelsPerUnit = 16;

    private new Camera camera;

    void Awake() {
        camera = GetComponent<Camera>();
    }

    void LateUpdate() {
        float verticalPixels = Screen.height / pixelScale;
        float scaleFactor = verticalPixels / pixelsPerUnit / 2f;
        camera.orthographicSize = scaleFactor;
    }
}
