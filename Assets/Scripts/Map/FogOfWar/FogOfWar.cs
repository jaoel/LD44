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
    public bool drawDebug = false;
    public SpriteRenderer fogOfWarRenderer;
    private float _viewRange = 15f;
    private MapGenerator _mapGenerator;
    private MaterialPropertyBlock _mpb;
    private int _textureID;
    private bool _enabled = true;
    private TileType[,] _tiles = new TileType[0, 0];
    private Vector3Int _size;
    private Vector3Int _origin;
    private CollisionShape _collisionShape;
    private Polygon2D _polygon = new Polygon2D();
    private List<Vector3> _shadowVerts = new List<Vector3>();

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

    public void GenerateFogOfWar()
    {
        GenerateTexture();

        _collisionShape = new CollisionShape();
        FindEdges();
    }

    private void GenerateTexture()
    {
        Tilemap floors = _mapGenerator.floors;
        Tilemap walls = _mapGenerator.walls;

        _size = new Vector3Int(Mathf.Max(floors.size.x, walls.size.x), Mathf.Max(floors.size.y, walls.size.y), 0);
        _origin = new Vector3Int(Mathf.Min(floors.origin.x, walls.origin.x), Mathf.Min(floors.origin.y, walls.origin.y), 0);
        FoWTexture = new Texture2D(_size.x, _size.y, TextureFormat.RGBA32, false, true);
        FoWTexture.wrapMode = TextureWrapMode.Clamp;

        _tiles = new TileType[_size.x, _size.y];
        for (int y = 0; y < _size.y; y++)
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

        Color32 resetColor = new Color32(255, 255, 0, 0);
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

    private bool IsWall(int x, int y)
    {
        if(x >= 0 && x < _tiles.GetLength(0) && y >= 0 && y < _tiles.GetLength(1))
        {
            return _tiles[x, y].HasFlag(TileType.Wall);
        }
        return false;
    }

    private void FindEdges()
    {
        float tileWidth = 1f;
        for(int y = 0; y < _tiles.GetLength(1); y++)
        {
            for(int x = 0; x < _tiles.GetLength(0); x++)
            {
                if(IsWall(x, y))
                {
                    float xx = x + _origin.x;
                    float yy = y + _origin.y;
                    Vector2 tl = new Vector2(tileWidth * xx, tileWidth * yy);
                    Vector2 tr = new Vector2(tileWidth + tileWidth * xx, tileWidth * yy);
                    Vector2 br = new Vector2(tileWidth + tileWidth * xx, tileWidth + tileWidth * yy);
                    Vector2 bl = new Vector2(tileWidth * xx, tileWidth + tileWidth * yy);
                    if (!IsWall(x, y - 1))
                    {
                        _collisionShape.AddEdge(tl, tr);
                    }
                    if (!IsWall(x, y + 1))
                    {
                        _collisionShape.AddEdge(bl, br);
                    }
                    if (!IsWall(x + 1, y))
                    {
                        _collisionShape.AddEdge(tr, br);
                    }
                    if (!IsWall(x - 1, y))
                    {
                        _collisionShape.AddEdge(tl, bl);
                    }
                }
            }
        }
        _collisionShape.Bake();
    }

    public void SetEnabled(bool enabled)
    {
        _enabled = enabled;
        fogOfWarRenderer.enabled = enabled;
    }

    private void Update()
    {
        if (drawDebug)
        {
            _collisionShape.DebugDraw();
        }

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
        Vector3Int playerTilePosition = WorldToTile(playerPosition - new Vector3(0.5f, 0.5f, 0f));
        int intViewRange = (int)_viewRange + 1;
        BoundsInt fowBounds = new BoundsInt(playerTilePosition - new Vector3Int(intViewRange, intViewRange, 0), new Vector3Int(2 * intViewRange + 1, 2 * intViewRange + 1, 0));

        _polygon = GetShadowPolygon(playerPosition);
        if (drawDebug)
        {
            _polygon.DebugDraw();
        }

        Vector2 originPos = playerPosition.ToVector2();
        float halfTextureWidth = FoWTexture.width / 2;
        float halfTextureHeight = FoWTexture.height / 2;
        float radius = 12f;

        float maxDistance = 2f;

        for (int y = Mathf.Min(fowBounds.yMax, _size.y - 1); y >= Mathf.Max(fowBounds.yMin, 0); y--)
        {
            for (int x = Mathf.Max(fowBounds.xMin, 0); x < Mathf.Min(fowBounds.xMax, _size.x); x++)
            {
                Vector2 tilePosition = TileToWorld(new Vector3Int(x, y, 0)).ToVector2() + new Vector2(0.5f, 0.5f);

                float polygonDist = Mathf.Clamp(_polygon.SignedClosestDistance(tilePosition), -maxDistance, maxDistance) / maxDistance;
                float circleDist = Mathf.Clamp((tilePosition - originPos).magnitude - radius, -maxDistance, maxDistance) / maxDistance;

                Color color;
                color.r = 0.5f + 0.5f * polygonDist;
                color.g = 0.5f + 0.5f * circleDist;
                color.b = circleDist < 0f && polygonDist > 0f && polygonDist < 0.5f && IsWall(x, y) ? 1f : 0f;
                color.a = 1f;

                if (polygonDist < 0f && !IsWall(x, y))
                {
                    if (IsWall(x + 1, y) || IsWall(x - 1, y) || IsWall(x, y + 1) || IsWall(x, y - 1) ||
                        IsWall(x + 1, y + 1) || IsWall(x + 1, y - 1) || IsWall(x - 1, y + 1) || IsWall(x - 1, y - 1))
                    {
                        color.b = 1f;
                    }
                }

                //if (y > 0f && IsWall(x, y - 1))
                //{
                //    Color color2 = FoWTexture.GetPixel(x, y - 1);
                //    if (color.b == 0f && color2.b > 0f)
                //    {
                //        color.b = 1f;
                //        //FoWTexture.SetPixel(x, y - 1, color2);
                //    }
                //}

                FoWTexture.SetPixel(x, y, color);
            }
        }

        SetTexture();
    }

    private Polygon2D GetShadowPolygon(Vector2 shadowOrigin)
    {
        float maxDistance = 20f;
        Polygon2D polygon = new Polygon2D();

        _shadowVerts = new List<Vector3>();

        foreach(var vert in _collisionShape.Vertices) {
            Vector2 rayDir = vert.position - shadowOrigin;
            float rayDistance = rayDir.magnitude;
            float dist = rayDistance;

            RaycastHit2D hit = Physics2D.Raycast(shadowOrigin, rayDir, dist, Layers.CombinedLayerMask(Layers.Map));
            if(hit)
            {
                dist = hit.distance;
            }


            if (Mathf.Approximately(dist, rayDistance))
            {
                // Store the angle in the vert's z-component
                _shadowVerts.Add(new Vector3(vert.position.x, vert.position.y, Vector2.SignedAngle(Vector2.right, vert.position - shadowOrigin)));
                if(vert.neighbours.Length > 2)
                {
                    continue;
                }

                // If we hit an edge we should check the angle to see if we need to continue casting past that edge
                Vector2 n0 = _collisionShape.Vertices[vert.neighbours[0]].position;
                Vector2 n1 = _collisionShape.Vertices[vert.neighbours[1]].position;
                Vector2 v0 = n0 - vert.position;
                Vector2 v1 = n1 - vert.position;
                float d0 = Vector2.Dot(rayDir, v0);
                float d1 = Vector2.Dot(rayDir, v1);

                // If the two edges' dot products are not the same sign, we have found a corner
                // If one of the dot products are 0, we are parallel to one of the edges
                // In any of these casese we need to offset the direction ray a bit, I use the normal of the corner to do this
                if (d0 < 0 != d1 < 0 || ((d0 == 0) != (d1 == 0)))
                {
                    Vector2 cornerNormal = vert.normal;
                    rayDir = rayDir.Rotate(rayDir.Cross(cornerNormal) >= 0f ? 0.001f : -0.001f);
                    Vector2 startPoint = vert.position + rayDir.normalized * 0.1f;

                    dist = maxDistance;
                    hit = Physics2D.Raycast(startPoint, rayDir, maxDistance, Layers.CombinedLayerMask(Layers.Map));
                    if(hit)
                    {
                        dist = hit.distance;
                    }

                    Vector2 cornerIntersectionPoint = startPoint + rayDir.normalized * dist;
                    _shadowVerts.Add(new Vector3(cornerIntersectionPoint.x, cornerIntersectionPoint.y, Vector2.SignedAngle(Vector2.right, cornerIntersectionPoint - vert.position)));
                }
            }
        }

        _shadowVerts.Sort((p0, p1) => {
            if (p0.z == p1.z)
            {
                return Vector3.Distance(shadowOrigin, p1).CompareTo(Vector3.Distance(shadowOrigin, p0));
            }
            else
            {
                return p0.z.CompareTo(p1.z);
            }
        });

        for (int i = 0; i < _shadowVerts.Count + 1; i++)
        {
            polygon.Vertices.Add(_shadowVerts[i % _shadowVerts.Count]);
        }

        return polygon;
    }

    private void OnDrawGizmos()
    {

        for (int i = 0; i < _shadowVerts.Count; i++)
        {
            Vector3 pos = _shadowVerts[i];
            if (i > 0 && _shadowVerts[i - 1].z >= _shadowVerts[i].z)
            {
                Vector3 p0 = _shadowVerts[i];
                Vector3 p1 = _shadowVerts[i - 1];
                p0.z = 0f;
                p1.z = 0f;

                Debug.DrawLine(p0 + 0.1f * Vector3.up, p1 + 0.1f * Vector3.up, Color.cyan);
                Debug.DrawLine(p0 + 0.1f * Vector3.left, p1 + 0.1f * Vector3.left, Color.cyan);
                Debug.DrawLine(p0 + 0.1f * Vector3.right, p1 + 0.1f * Vector3.right, Color.cyan);
                Debug.DrawLine(p0 + 0.1f * Vector3.down, p1 + 0.1f * Vector3.down, Color.cyan);

                Vector3 midPoint = Vector3.Lerp(_shadowVerts[i], _shadowVerts[i - 1], 0.5f);
                DrawString("Parallel", midPoint, Color.cyan);
            }
            DrawString(i.ToString() + "(" + pos.z + ")", pos);
        }
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

    static void DrawString(string text, Vector3 worldPos, Color? colour = null)
    {
        UnityEditor.Handles.BeginGUI();
        Color oldColor = GUI.color;
        if (colour.HasValue) GUI.color = colour.Value;
        var view = UnityEditor.SceneView.currentDrawingSceneView;
        if(view == null)
        {
            return;
        }
        Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);

        if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0)
        {
            UnityEditor.Handles.EndGUI();
            return;
        }

        Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
        GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 4, size.x, size.y), text);
        UnityEditor.Handles.EndGUI();
        GUI.color = oldColor;
    }
}
