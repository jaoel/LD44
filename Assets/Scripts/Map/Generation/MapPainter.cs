using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapPainter
{
    private TileContainer tileContainer;
    private Tilemap floors;
    private Tilemap walls;

    public void Initialize(TileContainer tileContainer_, Tilemap floors_, Tilemap walls_)
    {
        tileContainer = tileContainer_;
        floors = floors_;
        walls = walls_;
    }

    public void PaintRoom(RectInt cell, bool paintWalls)
    {
        Vector3Int pos = new Vector3Int(cell.xMin, cell.yMin, 0);
        Vector3Int size = new Vector3Int(cell.width, cell.height, 1);

        TileBase[] tiles = new TileBase[size.x * size.y];
        for (int i = 0; i < size.x * size.y; i++)
        {
            tiles[i] = tileContainer.FloorTiles[0];
        }

        floors.SetTilesBlock(new BoundsInt(pos, size), tiles);

        if (paintWalls)
        {
            for (int i = 0; i < size.x * size.y; i++)
            {
                tiles[i] = null;

                int x = i % size.x + pos.x;
                int y = i / size.x + pos.y;

                if (x == pos.x && y == pos.y)
                {
                    tiles[i] = tileContainer.BottomLeft;
                }
                else if (x == pos.x && y == cell.yMax - 1)
                {
                    tiles[i] = tileContainer.TopLeft;
                }
                else if (x == cell.xMax - 1 && y == pos.y)
                {
                    tiles[i] = tileContainer.BottomRight;
                }
                else if (x == cell.xMax - 1 && y == cell.yMax - 1)
                {
                    tiles[i] = tileContainer.TopRight;
                }
                else if (y == cell.yMax - 1)
                {
                    tiles[i] = tileContainer.TopMiddle;
                }
                else if (y == pos.y)
                {
                    tiles[i] = tileContainer.BottomMiddle;
                }
                else if (x == cell.xMax - 1)
                {
                    tiles[i] = tileContainer.MiddleRight;
                }
                else if (x == pos.x)
                {
                    tiles[i] = tileContainer.MiddleLeft;
                }
            }

            walls.SetTilesBlock(new BoundsInt(pos, size), tiles);
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
                walls.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
    }

    private bool RemoveThinWalls(int x, int y)
    {
        Vector3Int pos = new Vector3Int(x, y, 0);

        Tile middleRightWall = (Tile)walls.GetTile(pos + new Vector3Int(1, 0, 0));
        Tile middleLeftWall = (Tile)walls.GetTile(pos - new Vector3Int(1, 0, 0));
        Tile middleTopWall = (Tile)walls.GetTile(pos + new Vector3Int(0, 1, 0));
        Tile middleBottomWall = (Tile)walls.GetTile(pos - new Vector3Int(0, 1, 0));

        Tile middleLeftFloor = (Tile)floors.GetTile(pos - new Vector3Int(1, 0, 0));
        Tile middleRightFloor = (Tile)floors.GetTile(pos + new Vector3Int(1, 0, 0));
        Tile middleTopFloor = (Tile)floors.GetTile(pos + new Vector3Int(0, 1, 0));
        Tile middleBottomFloor = (Tile)floors.GetTile(pos - new Vector3Int(0, 1, 0));

        if (middleLeftFloor != null && middleRightFloor != null && middleRightWall == null && middleLeftWall == null)
        {
            walls.SetTile(pos, null);
            return true;
        }

        if (middleTopFloor != null && middleBottomFloor != null && middleTopWall == null && middleBottomWall == null)
        {
            walls.SetTile(pos, null);
            return true;
        }

        return false;
    }

    private Tile GetTileByNeighbours(int x, int y, bool recursive = false, bool ignoreCurrent = false)
    {
        TileBase currentFloorTile = floors.GetTile(new Vector3Int(x, y, 0));
        Tile currentWallTile = (Tile)walls.GetTile(new Vector3Int(x, y, 0));

        if (currentFloorTile == null)
        {
            return null;
        }

        if (currentWallTile != null && !ignoreCurrent)
        {
            return currentWallTile;
        }

        BoundsInt blockBounds = new BoundsInt(x - 1, y - 1, 0, 3, 3, 1);
        TileBase[] wallBlock = walls.GetTilesBlock(blockBounds);
        TileBase[] floorBlock = floors.GetTilesBlock(blockBounds);

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
                return tileContainer.MiddleLeft;
            }

            if (floorMiddleRight == null)
            {
                return tileContainer.MiddleRight;

            }
        }

        if (floorMiddleRight != null)
        {
            if (floorBottomMiddle == null && floorTopMiddle != null)
            {
                return tileContainer.BottomMiddle;
            }

            if (floorTopMiddle == null && floorBottomMiddle != null)
            {
                return tileContainer.TopMiddle;
            }
        }

        if (wallMiddleLeft != null)
        {
            if (wallMiddleLeft == tileContainer.BottomMiddle || wallMiddleLeft == tileContainer.TopLeftOuter)
            {
                if (wallBottomMiddle == tileContainer.MiddleLeft)
                {
                    return tileContainer.TopRightOuter;
                }

                if (floorMiddleRight == null && floorTopRight != null)
                {
                    return tileContainer.BottomRight;
                }
            }

            if (wallBottomMiddle != null && floorTopMiddle == null && floorMiddleRight == null)
            {
                return tileContainer.TopRight;
            }

            if (floorTopLeft == null && floorTopMiddle == null && floorTopRight == null && floorMiddleRight == null
                && floorBottomMiddle != null && wallBottomLeft == null)
            {
                return tileContainer.FloorTiles[3];
            }

            if (floorBottomLeft == null && wallTopMiddle == null && wallTopRight == null && wallBottomMiddle != null)
            {
                return tileContainer.TopRightOuter;
            }
        }

        if (wallBottomMiddle != null)
        {
            if (wallBottomMiddle == tileContainer.MiddleRight)
            {
                if (wallTopLeft != null && floorMiddleRight == null)
                {
                    return tileContainer.TopRight;
                }
            }

            if (floorBottomRight == null && wallMiddleLeft == null && wallBottomLeft == null)
            {
                return tileContainer.TopLeftOuter;
            }
        }

        if (wallMiddleLeft == null)
        {
            if (wallBottomMiddle == null && floorTopRight == null)
            {
                if (wallBottomRight != null)
                {
                    return tileContainer.BottomRightOuter;
                }

                if (wallBottomLeft == null && wallBottomRight == null && wallTopMiddle == null)
                {
                    return tileContainer.BottomRightOuter;
                }

                if (wallBottomLeft == null && wallBottomRight == null && wallTopMiddle != null)
                {
                    return tileContainer.BottomRightOuter;
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
                return tileContainer.TopLeftOuter;
            }

            if (GetTileByNeighbours(x + 1, y, true) != null && (wallBottomMiddle == tileContainer.MiddleRight
                || (wallBottomMiddle == tileContainer.BottomRight && floorBottomRight == null)))
            {
                return tileContainer.TopLeftOuter;
            }

            if (wallBottomMiddle == null && wallTopRight != tileContainer.TopMiddle && GetTileByNeighbours(x - 1, y + 1) == null
                && GetTileByNeighbours(x + 1, y, true) != null && GetTileByNeighbours(x, y + 1, true) != null && floorTopRight == null)
            {
                return tileContainer.BottomRightOuter;
            }
        }

        if (wallMiddleLeft != null)
        {
            if (wallBottomMiddle == null)
            {
                if ((GetTileByNeighbours(x, y + 1, true) == tileContainer.MiddleLeft || GetTileByNeighbours(x, y + 1, true) == tileContainer.MiddleRight))
                {
                    if (floorBottomMiddle == null)
                    {
                        return tileContainer.BottomRight;
                    }
                    else
                    {
                        return tileContainer.BottomLeftOuter;
                    }
                }

                if (floorTopLeft == null && wallMiddleRight == null && GetTileByNeighbours(x, y + 1, true) != null)
                {
                    return tileContainer.BottomLeftOuter;
                }
            }

            if ((wallMiddleLeft == tileContainer.BottomMiddle || wallMiddleLeft == tileContainer.BottomRight)
                && wallBottomMiddle == tileContainer.BottomLeft && GetTileByNeighbours(x + 1, y, true) == null
                && GetTileByNeighbours(x, y + 1, true) == null)
            {
                return tileContainer.TopRightOuter;
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
                if (RemoveThinWalls(x, y))
                {
                    continue;
                }

                Vector3Int pos = new Vector3Int(x, y, 0);
                Tile currentWallTile = (Tile)walls.GetTile(pos);

                Tile result = null;

                BoundsInt blockBounds = new BoundsInt(x - 1, y - 1, 0, 3, 3, 1);
                TileBase[] wallBlock = walls.GetTilesBlock(blockBounds);
                TileBase[] floorBlock = floors.GetTilesBlock(blockBounds);

                Tile floorBottomLeft = (Tile)floorBlock[0];
                Tile floorBottomMid = (Tile)floorBlock[1];
                Tile floorBottomRight = (Tile)floorBlock[2];
                Tile floorMiddleLeft = (Tile)floorBlock[3];
                Tile floorMiddleRight = (Tile)floorBlock[5];
                Tile floorTopLeft = (Tile)floorBlock[6];
                Tile floorTopMid = (Tile)floorBlock[7];
                Tile floorTopRight = (Tile)floorBlock[8];

                Tile wallBottomLeft = (Tile)wallBlock[0];
                Tile wallBottomMid = (Tile)wallBlock[1];
                Tile wallBottomRight = (Tile)wallBlock[2];
                Tile wallMiddleLeft = (Tile)wallBlock[3];
                Tile wallMiddleRight = (Tile)wallBlock[5];
                Tile wallTopLeft = (Tile)wallBlock[6];
                Tile wallTopMid = (Tile)wallBlock[7];
                Tile wallTopRight = (Tile)wallBlock[8];

                if (currentWallTile != null)
                {
                    if (wallBlock.Where(tile => tile != null).Count() <= 1)
                    {
                        walls.SetTile(new Vector3Int(x, y, 0), null);
                        continue;
                    }

                    if (wallMiddleLeft == null && wallBottomLeft == null && wallBottomMid == null
                        && wallMiddleRight != null && wallTopMid != null)
                    {
                        if (floorBottomMid == null)
                        {
                            result = tileContainer.BottomLeft;
                            walls.SetTile(new Vector3Int(x, y, 0), result);
                            continue;
                        }
                        else
                        {
                            result = tileContainer.BottomRightOuter;
                            walls.SetTile(new Vector3Int(x, y, 0), result);
                            continue;
                        }
                    }

                    if (wallMiddleLeft != null && wallBottomMid == null && wallBottomRight == null
                        && wallMiddleRight == null && wallTopMid != null)
                    {
                        if (floorBottomMid == null)
                        {
                            result = tileContainer.BottomRight;
                            walls.SetTile(new Vector3Int(x, y, 0), result);
                            continue;
                        }
                        else
                        {
                            if (wallMiddleLeft == tileContainer.TopLeftOuter)
                            {
                                result = tileContainer.BottomRight;
                                walls.SetTile(new Vector3Int(x, y, 0), result);
                                continue;
                            }
                            else
                            {
                                result = tileContainer.BottomLeftOuter;
                                walls.SetTile(new Vector3Int(x, y, 0), result);
                                continue;
                            }
                        }
                    }

                    if (wallMiddleLeft == null && wallBottomMid != null && wallMiddleRight != null && wallTopMid == null)
                    {
                        if (floorTopMid == null)
                        {
                            result = tileContainer.TopLeft;
                            walls.SetTile(new Vector3Int(x, y, 0), result);
                            continue;
                        }
                        else
                        {
                            result = tileContainer.TopLeftOuter;
                            walls.SetTile(new Vector3Int(x, y, 0), result);
                            continue;
                        }
                    }

                    if (wallMiddleLeft != null && wallBottomMid != null && wallMiddleRight == null)
                    {
                        if (wallTopMid == null)
                        {
                            if (floorTopMid == null)
                            {
                                result = tileContainer.TopRight;
                                walls.SetTile(new Vector3Int(x, y, 0), result);
                                continue;
                            }
                            else
                            {
                                result = tileContainer.TopRightOuter;
                                walls.SetTile(new Vector3Int(x, y, 0), result);
                                continue;
                            }
                        }
                        else
                        {
                            if (wallBottomMid == tileContainer.MiddleRight || wallBottomMid == tileContainer.BottomRightOuter)
                            {
                                result = tileContainer.TopRight;
                                walls.SetTile(new Vector3Int(x, y, 0), result);
                                continue;
                            }
                            else if ((wallMiddleLeft == tileContainer.BottomMiddle || wallMiddleLeft == tileContainer.TopLeftOuter)
                                && (wallTopMid == tileContainer.MiddleRight || wallTopMid == tileContainer.TopLeftOuter))
                            {
                                result = tileContainer.BottomRight;
                                walls.SetTile(new Vector3Int(x, y, 0), result);
                                continue;
                            }
                        }

                    }

                    if (wallMiddleLeft != null && wallBottomMid != null && wallMiddleRight != null)
                    {
                        if (floorTopMid == null && wallMiddleLeft == tileContainer.TopMiddle)
                        {
                            result = tileContainer.TopRight;
                            walls.SetTile(new Vector3Int(x, y, 0), result);
                            continue;
                        }
                        else if (floorTopMid == null && wallMiddleLeft == tileContainer.MiddleRight)
                        {
                            result = tileContainer.TopLeft;
                            walls.SetTile(new Vector3Int(x, y, 0), result);
                            continue;
                        }
                    }

                    if (wallMiddleLeft != null && wallBottomMid == null && wallMiddleRight != null)
                    {
                        if (floorTopMid != null && floorTopRight == null && floorBottomMid == null)
                        {
                            result = tileContainer.BottomRight;
                            walls.SetTile(new Vector3Int(x, y, 0), result);
                            continue;
                        }
                    }

                    if (currentWallTile != tileContainer.MiddleLeft && currentWallTile != tileContainer.MiddleRight
                        && currentWallTile != tileContainer.TopMiddle && currentWallTile != tileContainer.BottomMiddle)
                    {
                        if (wallBottomMid != null && floorMiddleRight != null && floorMiddleLeft == null && wallMiddleRight == null)
                        {
                            result = tileContainer.MiddleLeft;
                            walls.SetTile(new Vector3Int(x, y, 0), result);
                            continue;
                        }
                        else if (wallMiddleLeft != null && floorBottomMid != null && wallBottomMid == null)
                        {
                            result = tileContainer.TopMiddle;
                            walls.SetTile(new Vector3Int(x, y, 0), result);
                            continue;
                        }
                        else if (wallMiddleLeft != null && floorTopMid != null && wallTopMid == null)
                        {
                            result = tileContainer.BottomMiddle;
                            walls.SetTile(new Vector3Int(x, y, 0), result);
                            continue;
                        }
                        else if (wallBottomMid != null && floorMiddleLeft != null && floorMiddleRight == null)
                        {
                            result = tileContainer.MiddleRight;
                            walls.SetTile(new Vector3Int(x, y, 0), result);
                            continue;
                        }
                    }
                }
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

            if (floors.GetTilesBlock(limit).Any(x => x != null) && !walls.GetTilesBlock(limit).Any(x => x != null))
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

            if (floors.GetTilesBlock(limit).Any(x => x != null) && !walls.GetTilesBlock(limit).Any(x => x != null))
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

            if (floors.GetTilesBlock(limit).Any(x => x != null) && !walls.GetTilesBlock(limit).Any(x => x != null))
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

            if (floors.GetTilesBlock(limit).Any(x => x != null) && !walls.GetTilesBlock(limit).Any(x => x != null))
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

        floors.SetTilesBlock(new BoundsInt(pos, size), tiles);

        for (int i = 0; i < size.x * size.y; i++)
        {
            tiles[i] = null;

            int x = i % size.x + pos.x;
            int y = i / size.x + pos.y;

            if (x == pos.x && y == pos.y)
            {
                tiles[i] = tileContainer.BottomRightOuter;
            }
            else if (x == pos.x && y == cell.yMax - 1)
            {
                tiles[i] = tileContainer.TopLeftOuter;
            }
            else if (x == cell.xMax - 1 && y == pos.y)
            {
                tiles[i] = tileContainer.BottomLeftOuter;
            }
            else if (x == cell.xMax - 1 && y == cell.yMax - 1)
            {
                tiles[i] = tileContainer.TopRightOuter;
            }
            else if (y == cell.yMax - 1)
            {
                tiles[i] = tileContainer.BottomMiddle;
            }
            else if (y == pos.y)
            {
                tiles[i] = tileContainer.TopMiddle;
            }
            else if (x == cell.xMax - 1)
            {
                tiles[i] = tileContainer.MiddleLeft;
            }
            else if (x == pos.x)
            {
                tiles[i] = tileContainer.MiddleRight;
            }
        }

        walls.SetTilesBlock(new BoundsInt(pos, size), tiles);
    }
}
