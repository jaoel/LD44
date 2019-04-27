using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

    public SpriteRenderer spriteRenderer;

    private Color originalColor;
    private Vector3 originalSize;

    public BulletDescription description { get; set; }

    private void Start() {
        originalColor = spriteRenderer.color;
        originalSize = spriteRenderer.transform.localScale;
    }

    public void Reset() {
        SetTint(originalColor);
        SetSize(originalSize);
    }

    public void SetTint(Color color) {
        spriteRenderer.color = color;
    }

    public void SetSize(Vector2 size) {
        spriteRenderer.transform.localScale = new Vector3(size.x, size.y, 1f);
    }
}
