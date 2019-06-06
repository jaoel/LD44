using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(MapGenerator))]
public class FogOfWar : MonoBehaviour
{
    public SpriteRenderer fogOfWarRenderer;
    private float _viewRange = 15f;
    private MapGenerator _mapGenerator;
    private Texture2D _fowTexture;
    private MaterialPropertyBlock _mpb;
    private int _textureID;
    private bool _enabled = true;

    public void GenerateTexture()
    {
        Tilemap floors = _mapGenerator.floors;
        Tilemap walls = _mapGenerator.walls;


        Vector3Int size = new Vector3Int(Mathf.Max(floors.size.x, walls.size.x), Mathf.Max(floors.size.y, walls.size.y), 0);
        _fowTexture = new Texture2D(size.x, size.y, TextureFormat.RGBA32, false, true);
        Color32 resetColor = new Color32(0, 0, 0, 0);
        Color32[] resetColorArray = _fowTexture.GetPixels32();

        for (int i = 0; i < resetColorArray.Length; i++)
        {
            resetColorArray[i] = resetColor;
        }

        _fowTexture.SetPixels32(resetColorArray);


        fogOfWarRenderer.transform.localScale = Vector3.one;
        fogOfWarRenderer.transform.position = new Vector3(floors.origin.x, floors.origin.y, 0f);

        Sprite sprite = Sprite.Create(_fowTexture, new Rect(Vector2.zero, new Vector2(size.x, size.y)), Vector2.zero, 1f);
        fogOfWarRenderer.sprite = sprite;

        SetTexture();
    }

    public void SetEnabled(bool enabled)
    {
        _enabled = enabled;
        fogOfWarRenderer.enabled = enabled;
    }

    private void Update()
    {
        if (_enabled)
        {
            UpdateFogOfWar(Main.Instance.player.transform.position);
        }
    }

    public void UpdateFogOfWar(Vector3 playerPosition)
    {
        Tilemap floors = _mapGenerator.floors;
        Vector3Int tilePosition = floors.WorldToCell(playerPosition);
        int intViewRange = (int)_viewRange + 5;
        BoundsInt fowBounds = new BoundsInt(tilePosition - new Vector3Int(intViewRange, intViewRange, 0), new Vector3Int(2 * intViewRange, 2 * intViewRange, 0));
        for(int x = fowBounds.xMin; x < fowBounds.xMax; x++)
        {
            for(int y = fowBounds.yMin; y < fowBounds.yMax; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                {
                    Vector3Int pos = position - floors.origin;
                    float distance = Vector3.Distance(playerPosition, floors.CellToWorld(position));
                    Color pixelColor = _fowTexture.GetPixel(pos.x, pos.y);
                    Color targetColor = pixelColor;
                    float fuzzRange = 6f;
                    if(distance < _viewRange - fuzzRange)
                    {
                        targetColor.r = 1f;
                        targetColor.g = 1f;
                    }
                    else if(distance > _viewRange)
                    {
                        targetColor.r = 0f;
                    }
                    else
                    {
                        float frac = (_viewRange - distance) / fuzzRange;
                        targetColor.r = frac;
                        targetColor.g = Mathf.Max(targetColor.g, frac);
                    }

                    pixelColor = targetColor;
                    _fowTexture.SetPixel(pos.x, pos.y, pixelColor);
                }
            }
        }
        SetTexture();
    }

    private void Awake()
    {
        _mapGenerator = GetComponent<MapGenerator>();
        _mpb = new MaterialPropertyBlock();
        _textureID = Shader.PropertyToID("_MainTex");
    }

    private void SetTexture()
    {
        _fowTexture.Apply();
        fogOfWarRenderer.GetPropertyBlock(_mpb);
        _mpb.SetTexture(_textureID, _fowTexture);
        fogOfWarRenderer.SetPropertyBlock(_mpb);
    }
}
