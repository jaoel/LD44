using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class AutoScaleUI : MonoBehaviour
{
    private CanvasScaler _scaler;
    private int _lastHeight = 0;

    void Awake()
    {
        _scaler = GetComponent<CanvasScaler>();
        _scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        _scaler.referencePixelsPerUnit = 16;
    }

    private void Update()
    {
        if (_lastHeight != Screen.height)
        {
            AutoScale();
        }
        _lastHeight = Screen.height;
    }

    private void AutoScale()
    {
        if (Screen.height < 800)
        {
            _scaler.scaleFactor = 2;
            PixelPerfectCamera.pixelScale = 2;
        }
        else if (Screen.height < 1080)
        {
            _scaler.scaleFactor = 3;
            PixelPerfectCamera.pixelScale = 3;
        }
        else
        {
            _scaler.scaleFactor = 4;
            PixelPerfectCamera.pixelScale = 4;
        }
    }
}
