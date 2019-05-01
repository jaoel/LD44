using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class AutoScaleUI : MonoBehaviour {
    private CanvasScaler scaler;
    private int lastHeight = 0;

    void Awake() {
        scaler = GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.referencePixelsPerUnit = 16;
    }

    private void Update() {
        if (lastHeight != Screen.height) {
            AutoScale();
        }
        lastHeight = Screen.height;
    }

    private void AutoScale() {
        if (Screen.height < 800) {
            scaler.scaleFactor = 2;
        } else if (Screen.height < 1080) {
            scaler.scaleFactor = 3;
        } else {
            scaler.scaleFactor = 4;
        }
    }
}
