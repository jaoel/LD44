using UnityEngine;
using System.Collections;

public class HurtBlink : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer[] _spriteRenderers = null;

    private MaterialPropertyBlock _mpb;
    private int _colorID;
    private float _fadeTimestamp = 0f;
    private float _fadeDoneTimestamp = 0f;
    private Color _fadeColor = new Color(0f, 0f, 0f, 0f);
    private bool _fading = false;

    public void Blink(Color color, float duration)
    {
        if(duration > 0f)
        {
            _fadeColor = color;
            _fadeTimestamp = Time.time;
            _fadeDoneTimestamp = _fadeTimestamp + duration;
            _fading = true;
        }
    }

    private void Awake()
    {
        _mpb = new MaterialPropertyBlock();
        _colorID = Shader.PropertyToID("_Color");
    }

    private void Update()
    {
        if (_fading)
        {
            float time = Time.time;
            if (time >= _fadeTimestamp && time <= _fadeDoneTimestamp)
            {
                float duration = _fadeDoneTimestamp - _fadeTimestamp;
                float t = (time - _fadeTimestamp) / duration;
                t = 1f - Mathf.Clamp01(t);
                Color color = _fadeColor;
                color.a *= t;
                SetColor(color);
            }
            else if (_fadeColor.a != 0f)
            {
                _fadeColor.a = 0f;
                SetColor(_fadeColor);
                _fading = false;
            }
        }
    }

    private void SetColor(Color color)
    {
        if (_spriteRenderers != null)
        {
            foreach (SpriteRenderer spriteRenderer in _spriteRenderers)
            {
                spriteRenderer.GetPropertyBlock(_mpb);
                _mpb.SetColor(_colorID, color);
                spriteRenderer.SetPropertyBlock(_mpb);
            }
        }
    }
}
