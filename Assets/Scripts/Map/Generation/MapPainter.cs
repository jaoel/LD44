using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapPainter
{
    private TileContainer _wallContainer;
    private TileContainer _pitContainer;
    private Tilemap _floors;
    private Tilemap _walls;
    private Tilemap _pits;

    public void Initialize(TileContainer wallContainer, TileContainer pitContainer, Tilemap floors, Tilemap walls, Tilemap pits)
    {
        _wallContainer = wallContainer;
        _pitContainer = pitContainer;
        _floors = floors;
        _walls = walls;
        _pits = pits;
    }

    public void PaintRoom(RectInt cell, bool paintWalls)
    {
        Vector3Int pos = new Vector3Int(cell.xMin, cell.yMin, 0);
        Vector3Int size = new Vector3Int(cell.width, cell.height, 1);

        TileBase[] tiles = new TileBase[size.x * size.y];
        for (int i = 0; i < size.x * size.y; i++)
        {
            tiles[i] = _wallContainer.FloorTiles.GetRandom();// FloorTiles[0];
        }

        _floors.SetTilesBlock(new BoundsInt(pos, size), tiles);

        if (paintWalls)
        {
            for (int i = 0; i < size.x * size.y; i++)
            {
                tiles[i] = null;

                int x = i % size.x + pos.x;
                int y = i / size.x + pos.y;

                if (x == pos.x && y == pos.y)
                {
                    tiles[i] = _wallContainer.BottomLeft;
                }
                else if (x == pos.x && y == cell.yMax - 1)
                {
                    tiles[i] = _wallContainer.TopLeft;
                }
                else if (x == cell.xMax - 1 && y == pos.y)
                {
                    tiles[i] = _wallContainer.BottomRight;
                }
                else if (x == cell.xMax - 1 && y == cell.yMax - 1)
                {
                    tiles[i] = _wallContainer.TopRight;
                }
                else if (y == cell.yMax - 1)
                {
                    tiles[i] = _wallContainer.TopMiddle;
                }
                else if (y == pos.y)
                {
                    tiles[i] = _wallContainer.BottomMiddle;
                }
                else if (x == cell.xMax - 1)
                {
                    tiles[i] = _wallContainer.MiddleRight;
                }
                else if (x == pos.x)
                {
                    tiles[i] = _wallContainer.MiddleLeft;
                }
            }

            _walls.SetTilesBlock(new BoundsInt(pos, size), tiles);
        }
    }

    public void PaintPit(RectInt cell, bool paintWalls)
    {
        BoundsInt floorCheck = cell.ToBoundsInt();
        floorCheck.size += new Vector3Int(2, 2, 0);
        floorCheck.position -= new Vector3Int(1, 1, 0);

        TileBase[] floorTiles = _floors.GetTilesBlock(floorCheck);
        if (floorTiles.Any(x => x == null))
        {
            return;
        }

        Vector3Int pos = new Vector3Int(cell.xMin, cell.yMin, 0);
        Vector3Int size = new Vector3Int(cell.width, cell.height, 1);

        TileBase[] tiles = new TileBase[size.x * size.y];
        for (int i = 0; i < size.x * size.y; i++)
        {
            tiles[i] = _pitContainer.FloorTiles.GetRandom(); //FloorTiles[0];
        }

        _pits.SetTilesBlock(new BoundsInt(pos, size), tiles);

        if (paintWalls)
        {
            for (int i = 0; i < size.x * size.y; i++)
            {
                tiles[i] = null;

                int x = i % size.x + pos.x;
                int y = i / size.x + pos.y;

                if (x == pos.x && y == pos.y)
                {
                    tiles[i] = _pitContainer.BottomLeft;
                }
                else if (x == pos.x && y == cell.yMax - 1)
                {
                    tiles[i] = _pitContainer.TopLeft;
                }
                else if (x == cell.xMax - 1 && y == pos.y)
                {
                    tiles[i] = _pitContainer.BottomRight;
                }
                else if (x == cell.xMax - 1 && y == cell.yMax - 1)
                {
                    tiles[i] = _pitContainer.TopRight;
                }
                else if (y == cell.yMax - 1)
                {
                    tiles[i] = _pitContainer.TopMiddle;
                }
                else if (y == pos.y)
                {
                    tiles[i] = _pitContainer.BottomMiddle;
                }
                else if (x == cell.xMax - 1)
                {
                    tiles[i] = _pitContainer.MiddleRight;
                }
                else if (x == pos.x)
                {
                    tiles[i] = _pitContainer.MiddleLeft;
                }
            }

            _pits.SetTilesBlock(new BoundsInt(pos, size), tiles);
        }
    }

    public void PaintTiles(in Map map, in MapGeneratorParameters parameters)
    {
        for (int i = 0; i < map.Bounds.size.x; i++)
        {
            for (int j = 0; j < map.Bounds.size.y; j++)
            {
                int x = map.Bounds.xMin + i;
                int y = map.Bounds.yMin + j;
                RemoveThinWalls(x, y);

                Tile tile = GetTileByNeighbours(x, y);
                if (tile == null)
                {
                    continue;
                }

                map.CollisionMap[i, j] = 1;
                _walls.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
    }

    private bool RemoveThinWalls(int x, int y)
    {
        Vector3Int pos = new Vector3Int(x, y, 0);

        Tile middleRightWall = (Tile)_walls.GetTile(pos + new Vector3Int(1, 0, 0));
        Tile middleLeftWall = (Tile)_walls.GetTile(pos - new Vector3Int(1, 0, 0));
        Tile middleTopWall = (Tile)_walls.GetTile(pos + new Vector3Int(0, 1, 0));
        Tile middleBottomWall = (Tile)_walls.GetTile(pos - new Vector3Int(0, 1, 0));

        Tile middleLeftFloor = (Tile)_floors.GetTile(pos - new Vector3Int(1, 0, 0));
        Tile middleRightFloor = (Tile)_floors.GetTile(pos + new Vector3Int(1, 0, 0));
        Tile middleTopFloor = (Tile)_floors.GetTile(pos + new Vector3Int(0, 1, 0));
        Tile middleBottomFloor = (Tile)_floors.GetTile(pos - new Vector3Int(0, 1, 0));

        if (middleLeftFloor != null && middleRightFloor != null && middleRightWall == null && middleLeftWall == null)
        {
            _walls.SetTile(pos, null);
            return true;
        }

        if (middleTopFloor != null && middleBottomFloor != null && middleTopWall == null && middleBottomWall == null)
        {
            _walls.SetTile(pos, null);
            return true;
        }

        return false;
    }

    private Tile GetTileByNeighbours(int x, int y, bool recursive = false, bool ignoreCurrent = false)
    {
        TileBase currentFloorTile = _floors.GetTile(new Vector3Int(x, y, 0));
        Tile currentWallTile = (Tile)_walls.GetTile(new Vector3Int(x, y, 0));

        if (currentFloorTile == null)
        {
            return null;
        }

        if (currentWallTile != null && !ignoreCurrent)
        {
            return currentWallTile;
        }

        BoundsInt blockBounds = new BoundsInt(x - 1, y - 1, 0, 3, 3, 1);
        TileBase[] wallBlock = _walls.GetTilesBlock(blockBounds);
        TileBase[] floorBlock = _floors.GetTilesBlock(blockBounds);

        Tile floorBottomLeft = (Tile)floorBlock[0];
        Tile floorBottomMiddle = (Tile)floorBlock[1];
        Tile floorBottomRight = (Tile)floorBlock[2];
        Tile floorMiddleLeft = (Tile)floorBlock[3];
        Tile floorMiddleRight = (Tile)floorBlock[5];
        Tile floorTopLeft = (Tile)floorBlock[6];
        Tile floorTopMiddle = (Tile)floorBlock[7];
        Tile floorTopRight = (Tile)floorBlock[8];

        Tile wallBottomLeft = (Tile)wallBlock[0];
        Tile wallBottomMiddle = (Tile)wallBlock[1];
        Tile wallBottomRight = (Tile)wallBlock[2];
        Tile wallMiddleLeft = (Tile)wallBlock[3];
        Tile wallMiddleRight = (Tile)wallBlock[5];
        Tile wallTopLeft = (Tile)wallBlock[6];
        Tile wallTopMiddle = (Tile)wallBlock[7];
        Tile wallTopRight = (Tile)wallBlock[8];

        if (floorTopMiddle != null && floorBottomMiddle != null)
        {
            if (floorMiddleLeft == null)
            {
                return _wallContainer.MiddleLeft;
            }

            if (floorMiddleRight == null)
            {
                return _wallContainer.MiddleRight;

            }
        }

        if (floorMiddleRight != null)
        {
            if (floorBottomMiddle == null && floorTopMiddle != null)
            {
                return _wallContainer.BottomMiddle;
            }

            if (floorTopMiddle == null && floorBottomMiddle != null)
            {
                return _wallContainer.TopMiddle;
            }
        }

        if (wallMiddleLeft != null)
        {
            if (wallMiddleLeft == _wallContainer.BottomMiddle || wallMiddleLeft == _wallContainer.TopLeftOuter)
            {
                if (wallBottomMiddle == _wallContainer.MiddleLeft)
                {
                    return _wallContainer.TopRightOuter;
                }

                if (floorMiddleRight == null && floorTopRight != null)
                {
                    return _wallContainer.BottomRight;
                }
            }

            if (wallBottomMiddle != null && floorTopMiddle == null && floorMiddleRight == null)
            {
                return _wallContainer.TopRight;
            }

            //if (floorTopLeft == null && floorTopMiddle == null && floorTopRight == null && floorMiddleRight == null
            //    && floorBottomMiddle != null && wallBottomLeft == null)
            //{
            //    return _wallContainer.FloorTiles[3];
            //}

            if (floorBottomLeft == null && wallTopMiddle == null && wallTopRight == null && wallBottomMiddle != null)
            {
                return _wallContainer.TopRightOuter;
            }
        }

        if (wallBottomMiddle != null)
        {
            if (wallBottomMiddle == _wallContainer.MiddleRight)
            {
                if (wallTopLeft != null && floorMiddleRight == null)
                {
                    return _wallContainer.TopRight;
                }
            }

            if (floorBottomRight == null && wallMiddleLeft == null && wallBottomLeft == null)
            {
                return _wallContainer.TopLeftOuter;
            }
        }

        if (wallMiddleLeft == null)
        {
            if (wallBottomMiddle == null && floorTopRight == null)
            {
                if (wallBottomRight != null)
                {
                    return _wallContainer.BottomRightOuter;
                }

                if (wallBottomLeft == null && wallBottomRight == null && wallTopMiddle == null)
                {
                    return _wallContainer.BottomRightOuter;
                }

                if (wallBottomLeft == null && wallBottomRight == null && wallTopMiddle != null)
                {
                    return _wallContainer.BottomRightOuter;
                }
            }
        }

        if (recursive)
        {
            return null;
        }

        if (wallMiddleLeft == null)
        {
            if (GetTileByNeighbours(x + 1, y, true) != null && floorBottomRight == null)
            {
                return _wallContainer.TopLeftOuter;
            }

            if (GetTileByNeighbours(x + 1, y, true) != null && (wallBottomMiddle == _wallContainer.MiddleRight
                || (wallBottomMiddle == _wallContainer.BottomRight && floorBottomRight == null)))
            {
                return _wallContainer.TopLeftOuter;
            }

            if (wallBottomMiddle == null && wallTopRight != _wallContainer.TopMiddle && GetTileByNeighbours(x - 1, y + 1) == null
                && GetTileByNeighbours(x + 1, y, true) != null && GetTileByNeighbours(x, y + 1, true) != null && floorTopRight == null)
            {
                return _wallContainer.BottomRightOuter;
            }
        }

        if (wallMiddleLeft != null)
        {
            if (wallBottomMiddle == null)
            {
                if ((GetTileByNeighbours(x, y + 1, true) == _wallContainer.MiddleLeft || GetTileByNeighbours(x, y + 1, true) == _wallContainer.MiddleRight))
                {
                    if (floorBottomMiddle == null)
                    {
                        return _wallContainer.BottomRight;
                    }
                    else
                    {
                        return _wallContainer.BottomLeftOuter;
                    }
                }

                if (floorTopLeft == null && wallMiddleRight == null && GetTileByNeighbours(x, y + 1, true) != null)
                {
                    return _wallContainer.BottomLeftOuter;
                }
            }

            if ((wallMiddleLeft == _wallContainer.BottomMiddle || wallMiddleLeft == _wallContainer.BottomRight)
                && wallBottomMiddle == _wallContainer.BottomLeft && GetTileByNeighbours(x + 1, y, true) == null
                && GetTileByNeighbours(x, y + 1, true) == null)
            {
                return _wallContainer.TopRightOuter;
            }
        }

        return null;
    }

    public void PostProcessTiles(in Map map, in MapGeneratorParameters parameters)
    {
        for (int x = map.Bounds.xMin; x < map.Bounds.xMax; x++)
        {
            for (int y = map.Bounds.yMin; y < map.Bounds.yMax; y++)
            {
                bool paintWalls = true;
                if (RemoveThinWalls(x, y))
                {
                    paintWalls = true;
                }

                Vector3Int pos = new Vector3Int(x, y, 0);
                Tile currentWallTile = (Tile)_walls.GetTile(pos);
                Tile currentPitTile = (Tile)_pits.GetTile(pos);

                if (!paintWalls && currentPitTile == null)
                {
                    continue;
                }

                BoundsInt blockBounds = new BoundsInt(x - 1, y - 1, 0, 3, 3, 1);
                TileBase[] floorBlock = _floors.GetTilesBlock(blockBounds);

                Tile floorBottomLeft = (Tile)floorBlock[0];
                Tile floorBottomMid = (Tile)floorBlock[1];
                Tile floorBottomRight = (Tile)floorBlock[2];
                Tile floorMiddleLeft = (Tile)floorBlock[3];
                Tile floorMiddleRight = (Tile)floorBlock[5];
                Tile floorTopLeft = (Tile)floorBlock[6];
                Tile floorTopMid = (Tile)floorBlock[7];
                Tile floorTopRight = (Tile)floorBlock[8];

                if (paintWalls && currentWallTile != null)
                {
                    TileBase[] wallBlock = _walls.GetTilesBlock(blockBounds);
                    PostProcessTile(x, y, _walls, _wallContainer, currentWallTile, wallBlock, floorBottomMid, 
                        floorMiddleLeft, floorMiddleRight, floorTopMid, floorTopRight);
                }
                
                if (currentPitTile != null)
                {
                    PostProcessPits(x, y, blockBounds);
                }
            }
        }

        map.UpdateCollisionMapDebug(false);
    }

    private void PostProcessPits(int x, int y, BoundsInt blockBounds)
    {
        TileBase[] pitBlock = _pits.GetTilesBlock(blockBounds);

        Tile bottomLeft = (Tile)pitBlock[0];
        Tile bottomMid = (Tile)pitBlock[1];
        Tile bottomRight = (Tile)pitBlock[2];
        Tile middleLeft = (Tile)pitBlock[3];
        Tile middleRight = (Tile)pitBlock[5];
        Tile topLeft = (Tile)pitBlock[6];
        Tile topMid = (Tile)pitBlock[7];
        Tile topRight = (Tile)pitBlock[8];

        if (topMid == null && middleRight != null && bottomMid != null && middleLeft == null)
        {
            _pits.SetTile(new Vector3Int(x, y, 0), _pitContainer.TopLeft);
            return;
        }

        if (topMid == null && middleRight == null && bottomMid != null && middleLeft != null)
        {
            _pits.SetTile(new Vector3Int(x, y, 0), _pitContainer.TopRight);
            return;
        }

        if (middleLeft == null && bottomMid == null && topMid != null && middleRight != null)
        {
            _pits.SetTile(new Vector3Int(x, y, 0), _pitContainer.BottomLeft);
            return;
        }

        if (middleLeft != null && bottomMid == null && topMid != null && middleRight == null)
        {
            _pits.SetTile(new Vector3Int(x, y, 0), _pitContainer.BottomRight);
            return;
        }

        if (middleLeft != null && middleRight != null && topMid == null && bottomMid != null)
        {
            _pits.SetTile(new Vector3Int(x, y, 0), _pitContainer.TopMiddle);
            return;
        }

        if (middleLeft != null && middleRight != null && topMid != null && bottomMid == null)
        {
            _pits.SetTile(new Vector3Int(x, y, 0), _pitContainer.BottomMiddle);
            return;
        }

        if (middleLeft != null && middleRight == null && topMid != null && bottomMid != null)
        {
            _pits.SetTile(new Vector3Int(x, y, 0), _pitContainer.MiddleRight);
            return;
        }

        if (middleLeft == null && middleRight != null && topMid != null && bottomMid != null)
        {
            _pits.SetTile(new Vector3Int(x, y, 0), _pitContainer.MiddleLeft);
            return;
        }

        if (bottomLeft == null && middleLeft != null && bottomMid != null)
        {
            _pits.SetTile(new Vector3Int(x, y, 0), _pitContainer.BottomLeftOuter);
            return;
        }

        if (bottomRight == null && middleRight != null && bottomMid != null)
        {
            _pits.SetTile(new Vector3Int(x, y, 0), _pitContainer.BottomRightOuter);
            return;
        }

        if (topLeft == null && middleLeft != null && topMid != null)
        {
            _pits.SetTile(new Vector3Int(x, y, 0), _pitContainer.TopLeftOuter);
            return;
        }

        if (topRight == null && middleRight != null && topMid != null)
        {
            _pits.SetTile(new Vector3Int(x, y, 0), _pitContainer.TopRightOuter);
            return;
        }
    }

    private void PostProcessTile(int x, int y, Tilemap target, TileContainer tileContainer, Tile currentTile, TileBase[] tileBlock, 
        Tile floorBottomMid, Tile floorMiddleLeft, Tile floorMiddleRight, Tile floorTopMid, Tile floorTopRight)
    {
        Tile result = null;

        Tile bottomLeft = (Tile)tileBlock[0];
        Tile bottomMid = (Tile)tileBlock[1];
        Tile bottomRight = (Tile)tileBlock[2];
        Tile middleLeft = (Tile)tileBlock[3];
        Tile middleRight = (Tile)tileBlock[5];
        Tile topLeft = (Tile)tileBlock[6];
        Tile topMid = (Tile)tileBlock[7];
        Tile topRight = (Tile)tileBlock[8];

        if (tileBlock.Where(tile => tile != null).Count() <= 1)
        {
            target.SetTile(new Vector3Int(x, y, 0), null);
            return;
        }

        if (middleLeft == null && bottomLeft == null && bottomMid == null
            && middleRight != null && topMid != null)
        {
            if (floorBottomMid == null)
            {
                result = tileContainer.BottomLeft;
                target.SetTile(new Vector3Int(x, y, 0), result);
                return;
            }
            else
            {
                result = tileContainer.BottomRightOuter;
                target.SetTile(new Vector3Int(x, y, 0), result);
                return;
            }
        }

        if (middleLeft != null && bottomMid == null && bottomRight == null
            && middleRight == null && topMid != null)
        {
            if (floorBottomMid == null)
            {
                result = tileContainer.BottomRight;
                target.SetTile(new Vector3Int(x, y, 0), result);
                return;
            }
            else
            {
                if (middleLeft == tileContainer.TopLeftOuter)
                {
                    result = tileContainer.BottomRight;
                    target.SetTile(new Vector3Int(x, y, 0), result);
                    return;
                }
                else
                {
                    result = tileContainer.BottomLeftOuter;
                    target.SetTile(new Vector3Int(x, y, 0), result);
                    return;
                }
            }
        }

        if (middleLeft == null && bottomMid != null && middleRight != null && topMid == null)
        {
            if (floorTopMid == null)
            {
                result = tileContainer.TopLeft;
                target.SetTile(new Vector3Int(x, y, 0), result);
                return;
            }
            else
            {
                result = tileContainer.TopLeftOuter;
                target.SetTile(new Vector3Int(x, y, 0), result);
                return;
            }
        }

        if (middleLeft != null && bottomMid != null && middleRight == null)
        {
            if (topMid == null)
            {
                if (floorTopMid == null)
                {
                    result = tileContainer.TopRight;
                    target.SetTile(new Vector3Int(x, y, 0), result);
                    return;
                }
                else
                {
                    result = tileContainer.TopRightOuter;
                    target.SetTile(new Vector3Int(x, y, 0), result);
                    return;
                }
            }
            else
            {
                if (bottomMid == tileContainer.MiddleRight || bottomMid == tileContainer.BottomRightOuter)
                {
                    result = tileContainer.TopRight;
                    target.SetTile(new Vector3Int(x, y, 0), result);
                    return;
                }
                else if ((middleLeft == tileContainer.BottomMiddle || middleLeft == tileContainer.TopLeftOuter)
                    && (topMid == tileContainer.MiddleRight || topMid == tileContainer.TopLeftOuter))
                {
                    result = tileContainer.BottomRight;
                    target.SetTile(new Vector3Int(x, y, 0), result);
                    return;
                }
            }

        }

        if (middleLeft != null && bottomMid != null && middleRight != null)
        {
            if (floorTopMid == null && middleLeft == tileContainer.TopMiddle)
            {
                result = tileContainer.TopRight;
                target.SetTile(new Vector3Int(x, y, 0), result);
                return;
            }
            else if (floorTopMid == null && middleLeft == tileContainer.MiddleRight)
            {
                result = tileContainer.TopLeft;
                target.SetTile(new Vector3Int(x, y, 0), result);
                return;
            }
        }

        if (middleLeft != null && bottomMid == null && middleRight != null)
        {
            if (floorTopMid != null && floorTopRight == null && floorBottomMid == null)
            {
                result = tileContainer.BottomRight;
                target.SetTile(new Vector3Int(x, y, 0), result);
                return;
            }
        }

        if (currentTile != tileContainer.MiddleLeft && currentTile != tileContainer.MiddleRight
            && currentTile != tileContainer.TopMiddle && currentTile != tileContainer.BottomMiddle)
        {
            if (bottomMid != null && floorMiddleRight != null && floorMiddleLeft == null && middleRight == null)
            {
                result = tileContainer.MiddleLeft;
                target.SetTile(new Vector3Int(x, y, 0), result);
                return;
            }
            else if (middleLeft != null && floorBottomMid != null && bottomMid == null)
            {
                result = tileContainer.TopMiddle;
                target.SetTile(new Vector3Int(x, y, 0), result);
                return;
            }
            else if (middleLeft != null && floorTopMid != null && topMid == null)
            {
                result = tileContainer.BottomMiddle;
                target.SetTile(new Vector3Int(x, y, 0), result);
                return;
            }
            else if (bottomMid != null && floorMiddleLeft != null && floorMiddleRight == null)
            {
                result = tileContainer.MiddleRight;
                target.SetTile(new Vector3Int(x, y, 0), result);
                return;
            }
        }
    }


    public void BuildVerticalDoorWalls(Map map, MapNode target, BoundsInt chokepoint)
    {
        BoundsInt bounds = chokepoint;
        bounds.yMin += 3;
        BoundsInt drawArea = new BoundsInt(bounds.position, bounds.size);
        bounds.yMax -= 1;

        //Chokepoint is on the left edge
        if (chokepoint.center.x < target.Cell.center.x)
        {
            BoundsInt limit = bounds;
            limit.x -= 2;

            if (_floors.GetTilesBlock(limit).Any(x => x != null) && !_walls.GetTilesBlock(limit).Any(x => x != null))
            {
                drawArea.xMin -= 1;
                PaintBox(drawArea.ToRectInt());
            }
            else
            {
                drawArea.xMax += 1;
                PaintBox(drawArea.ToRectInt());
            }

            map.UpdateCollisionMap(drawArea.ToRectInt(), 1);
        }
        //chokepoint is on the right edge
        else
        {
            BoundsInt limit = bounds;
            limit.x += 2;

            if (_floors.GetTilesBlock(limit).Any(x => x != null) && !_walls.GetTilesBlock(limit).Any(x => x != null))
            {
                drawArea.xMax += 1;
                PaintBox(drawArea.ToRectInt());
            }
            else
            {
                drawArea.xMin -= 1;
                PaintBox(drawArea.ToRectInt());
            }

            map.UpdateCollisionMap(drawArea.ToRectInt(), 1);
        }
    }

    public void BuildHorizontalDoorWalls(Map map, MapNode target, BoundsInt chokepoint)
    {
        BoundsInt bounds = new BoundsInt(chokepoint.position, chokepoint.size);
        bounds.xMin += 3;

        BoundsInt drawArea = new BoundsInt(bounds.position, bounds.size);

        bounds.xMax -= 1;

        Tile[] tileBlock = new Tile[drawArea.size.x * drawArea.size.y];

        //Chokepoint is on the lower edge
        if (chokepoint.center.y < target.Cell.center.y)
        {
            BoundsInt limit = bounds;
            limit.y -= 2;

            if (_floors.GetTilesBlock(limit).Any(x => x != null) && !_walls.GetTilesBlock(limit).Any(x => x != null))
            {
                drawArea.yMin -= 1;
                PaintBox(drawArea.ToRectInt());
                map.UpdateCollisionMap(drawArea.ToRectInt(), 1);
            }
            else
            {
                drawArea.yMax += 1;
                PaintBox(drawArea.ToRectInt());
                map.UpdateCollisionMap(drawArea.ToRectInt(), 1);
            }
        }
        //chokepoint is on the upper edge
        else
        {
            BoundsInt limit = bounds;
            limit.y += 2;

            if (_floors.GetTilesBlock(limit).Any(x => x != null) && !_walls.GetTilesBlock(limit).Any(x => x != null))
            {
                drawArea.yMax += 1;
                PaintBox(drawArea.ToRectInt());
                map.UpdateCollisionMap(drawArea.ToRectInt(), 1);
            }
            else
            {
                drawArea.yMin -= 1;
                PaintBox(drawArea.ToRectInt());
                map.UpdateCollisionMap(drawArea.ToRectInt(), 1);
            }
        }
    }

    public void PaintBox(RectInt cell)
    {
        Vector3Int pos = new Vector3Int(cell.xMin, cell.yMin, 0);
        Vector3Int size = new Vector3Int(cell.width, cell.height, 1);

        TileBase[] tiles = new TileBase[size.x * size.y];
        for (int i = 0; i < size.x * size.y; i++)
        {
            tiles[i] = null;// tileContainer.FloorTiles[0];
        }

        _floors.SetTilesBlock(new BoundsInt(pos, size), tiles);

        for (int i = 0; i < size.x * size.y; i++)
        {
            tiles[i] = null;

            int x = i % size.x + pos.x;
            int y = i / size.x + pos.y;

            if (x == pos.x && y == pos.y)
            {
                tiles[i] = _wallContainer.BottomRightOuter;
            }
            else if (x == pos.x && y == cell.yMax - 1)
            {
                tiles[i] = _wallContainer.TopLeftOuter;
            }
            else if (x == cell.xMax - 1 && y == pos.y)
            {
                tiles[i] = _wallContainer.BottomLeftOuter;
            }
            else if (x == cell.xMax - 1 && y == cell.yMax - 1)
            {
                tiles[i] = _wallContainer.TopRightOuter;
            }
            else if (y == cell.yMax - 1)
            {
                tiles[i] = _wallContainer.BottomMiddle;
            }
            else if (y == pos.y)
            {
                tiles[i] = _wallContainer.TopMiddle;
            }
            else if (x == cell.xMax - 1)
            {
                tiles[i] = _wallContainer.MiddleLeft;
            }
            else if (x == pos.x)
            {
                tiles[i] = _wallContainer.MiddleRight;
            }
        }

        _walls.SetTilesBlock(new BoundsInt(pos, size), tiles);
    }
}
