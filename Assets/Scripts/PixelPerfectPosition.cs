﻿using UnityEngine;

public class PixelPerfectPosition : MonoBehaviour
{
    private Vector3 _offset;
    private void Awake()
    {
        _offset = transform.localPosition;
    }

    void LateUpdate()
    {
        int pixelsPerUnit = PixelPerfectCamera.pixelsPerUnit;
        Vector3 position = transform.localPosition;

        position.x = (Mathf.Round(transform.parent.position.x * pixelsPerUnit) / pixelsPerUnit) - transform.parent.position.x;
        position.y = (Mathf.Round(transform.parent.position.y * pixelsPerUnit) / pixelsPerUnit) - transform.parent.position.y;
        position.z = 0f;

        transform.localPosition = position + _offset;
    }
}
