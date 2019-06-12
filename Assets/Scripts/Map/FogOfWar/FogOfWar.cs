using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Flags]
public enum TileType
{
    None = 0,
    Floor = 1,
    Wall = 2,
}

[RequireComponent(typeof(MapGenerator))]
public class FogOfWar : MonoBehaviour
{
    public SpriteRenderer fogOfWarRenderer;
    private float _viewRange = 15f;
    private MapGenerator _mapGenerator;
    private MaterialPropertyBlock _mpb;
    private int _textureID;
    private bool _enabled = true;
    private TileType[,] _tiles = new TileType[0, 0];
    private Vector3Int _size;
    private Vector3Int _origin;

    public Texture2D FoWTexture { get; private set; }

    private static FogOfWar _instance = null;
    public static FogOfWar Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }

            _instance = FindObjectOfType<FogOfWar>();

            if (_instance == null || _instance.Equals(null))
            {
                Debug.LogError("The scene needs a FogOfWar component");
            }
            return _instance;
        }
    }

    public void GenerateTexture()
    {
        Tilemap floors = _mapGenerator.floors;
        Tilemap walls = _mapGenerator.walls;

        _size = new Vector3Int(Mathf.Max(floors.size.x, walls.size.x), Mathf.Max(floors.size.y, walls.size.y), 0);
        _origin = new Vector3Int(Mathf.Min(floors.origin.x, walls.origin.x), Mathf.Min(floors.origin.y, walls.origin.y), 0);
        FoWTexture = new Texture2D(_size.x, _size.y, TextureFormat.RGBA32, false, true);
        FoWTexture.wrapMode = TextureWrapMode.Clamp;

        _tiles = new TileType[_size.x, _size.y];
        for(int y = 0; y < _size.y; y++)
        {
            for (int x = 0; x < _size.x; x++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                _tiles[x, y] = TileType.None;

                if (floors.HasTile(position + _origin))
                {
                    _tiles[x, y] |= TileType.Floor;
                }

                if (walls.HasTile(position + _origin))
                {
                    _tiles[x, y] |= TileType.Wall;
                }
            }
        }

        Color32 resetColor = new Color32(0, 0, 0, 0);
        Color32[] resetColorArray = FoWTexture.GetPixels32();

        for (int i = 0; i < resetColorArray.Length; i++)
        {
            resetColorArray[i] = resetColor;
        }

        FoWTexture.SetPixels32(resetColorArray);


        fogOfWarRenderer.transform.localScale = Vector3.one;
        fogOfWarRenderer.transform.position = _origin;

        Sprite sprite = Sprite.Create(FoWTexture, new Rect(Vector2.zero, new Vector2(_size.x, _size.y)), Vector2.zero, 1f);
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

    public Vector3Int WorldToTile(Vector3 worldPos)
    {
        return worldPos.ToVector3Int() - _origin;
    }

    public Vector3 TileToWorld(Vector3Int tilePos)
    {
        return tilePos + _origin;
    }

    public void UpdateFogOfWar(Vector3 playerPosition)
    {
        Vector3Int tilePosition = WorldToTile(playerPosition - new Vector3(0.5f, 0.5f, 0f));
        int intViewRange = (int)_viewRange + 1;
        BoundsInt fowBounds = new BoundsInt(tilePosition - new Vector3Int(intViewRange, intViewRange, 0), new Vector3Int(2 * intViewRange + 1, 2 * intViewRange + 1, 0));

        float[,] visibility = new float[fowBounds.size.x, fowBounds.size.y];
        ShadowCast.CalculateVisibility(playerPosition - _origin, fowBounds, _tiles, visibility);

        for (int x = fowBounds.xMin; x < fowBounds.xMax; x++)
        {
            if (x < 0 || x >= _size.x)
            {
                continue;
            }

            for (int y = fowBounds.yMin; y < fowBounds.yMax; y++)
            {
                if (y < 0 || y >= _size.y)
                {
                    continue;
                }

                Vector3Int pos = new Vector3Int(x, y, 0);
                TileType tileType = _tiles[x, y];

                float distance = Vector3.Distance(playerPosition - new Vector3(0.5f, 0.5f, 0f), TileToWorld(pos));
                Color pixelColor = FoWTexture.GetPixel(pos.x, pos.y);
                Color targetColor = pixelColor;
                float fuzzRange = 2f;
                float vis = visibility[x - fowBounds.xMin, y - fowBounds.yMin];
                if (distance < _viewRange - fuzzRange)
                {
                    targetColor.r = 1f;
                    //targetColor.g = 1f;
                }
                else if (distance > _viewRange)
                {
                    targetColor.r = 0f;
                }
                else
                {
                    float frac = (_viewRange - distance) / fuzzRange;
                    targetColor.r = frac;
                    //targetColor.g = Mathf.Max(pixelColor.g, frac);
                }

                targetColor.r *= vis;
                targetColor.g = Mathf.Max(targetColor.g, targetColor.r);

                if (targetColor.r > 0f)
                {
                    if(x == tilePosition.x && y == tilePosition.y)
                    {
                        targetColor.b = 1.0f;
                    }
                    else if (tileType.HasFlag(TileType.Wall))
                    {
                        targetColor.b = 0.75f;
                    }
                    else if (tileType.HasFlag(TileType.Floor))
                    {
                        targetColor.b = 0.5f;
                    }
                }

                FoWTexture.SetPixel(pos.x, pos.y, targetColor);
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
        FoWTexture.Apply();
        fogOfWarRenderer.GetPropertyBlock(_mpb);
        _mpb.SetTexture(_textureID, FoWTexture);
        fogOfWarRenderer.SetPropertyBlock(_mpb);
    }
}
