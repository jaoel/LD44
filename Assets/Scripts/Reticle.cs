using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Reticle : MonoBehaviour
{
    RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        rectTransform.anchoredPosition = Input.mousePosition / PixelPerfectCamera.pixelScale;
        //Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //position.z = 0.0f;
        //transform.position = position;
    }
} 
