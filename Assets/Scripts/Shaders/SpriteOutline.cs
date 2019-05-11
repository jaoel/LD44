using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteOutline : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private GameObject[] outlines;
    private SpriteRenderer[] outlineRenderers;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        outlines = new GameObject[4];
        outlineRenderers = new SpriteRenderer[4];

        float upp = 1f / spriteRenderer.sprite.pixelsPerUnit;
        Vector3[] positions = {
            new Vector3(upp, 0f, 0f),
            new Vector3(-upp, 0f, 0f),
            new Vector3(0f, upp, 0f),
            new Vector3(0f, -upp, 0f),
        };
        for(int i = 0; i < outlines.Length; i++)
        {
            outlines[i] = Instantiate(gameObject, transform);
            foreach(Transform child in outlines[i].transform)
            {
                Destroy(child.gameObject);
            }
            Destroy(outlines[i].GetComponent<SpriteOutline>());
            Destroy(outlines[i].GetComponent<PixelPerfectPosition>());
            outlines[i].name = "Outline";
            outlines[i].transform.localPosition = positions[i];
            outlineRenderers[i] = outlines[i].GetComponent<SpriteRenderer>();
            outlineRenderers[i].color = Color.black;
            outlineRenderers[i].sortingOrder--;
        }
    }

    private void LateUpdate()
    {
        for (int i = 0; i < outlineRenderers.Length; i++)
        {
            outlineRenderers[i].sprite = spriteRenderer.sprite;
        }
    }
}
