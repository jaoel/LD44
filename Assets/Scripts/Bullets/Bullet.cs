using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

    public SpriteRenderer spriteRenderer;

    private Color originalColor;
    private Vector3 originalSize;

    public BulletDescription description { get; set; }
    private GameObject _owner;

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

    public void SetOwner(GameObject owner)
    {
        _owner = owner;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject == _owner)
            return;

        if (collision.gameObject.layer == LayerContainer.Instance.Layers["Map"])
        {
        }
        else if (collision.gameObject.layer == LayerContainer.Instance.Layers["Enemy"])
        {
            collision.gameObject.GetComponent<Enemy>().ApplyDamage(description.damage);
            CameraManager.Instance.ShakeCamera(1.0f, 0.2f, 0.3f);
        }
        else if (collision.gameObject.layer == LayerContainer.Instance.Layers["Player"])
        {
            collision.gameObject.GetComponent<Player>().ReceiveDamage(description.damage);
        }

        gameObject.SetActive(false);
    }
}
