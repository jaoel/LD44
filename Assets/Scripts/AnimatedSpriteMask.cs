using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteMask), typeof(SpriteRenderer))]
public class AnimatedSpriteMask : MonoBehaviour
{
    private SpriteMask _spriteMask;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteMask = GetComponent<SpriteMask>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if(_spriteMask.sprite != _spriteRenderer.sprite)
        {
            _spriteMask.sprite = _spriteRenderer.sprite;
        }
    }
}
