using System;
using System.Collections.Generic;
using UnityEngine;

// Archived cellular-automata generator retained as a comparison against the current BSP approach.
// It creates a random wall field, smooths that field through neighbour counts, keeps the largest
// connected floor region, and marks representative boundary cells as doors.
public class CellularAutomataDungeonGenerator : DungeonGridGenerator2D
{
  [Header("Cellular Automata Settings")]
  [SerializeField, Range(0, 100)] int initialWallChance = 45;
  [SerializeField, Min(1)] int smoothingPasses = 5;

  // Clamps the smoothing pass count after applying the shared grid validation.
  protected override void OnValidate()
  {
    base.OnValidate();
    smoothingPasses = Mathf.Max(1, smoothingPasses);
  }

  // Builds the seeded wall field, smooths it, keeps its main floor region, and marks doors.
  protected override TileType[,] BuildMap(int seed)
  {
    System.Random random = new System.Random(seed);
    TileType[,] map = CreateFilledMap(TileType.Wall);

    for (int x = 1; x < GridWidth - 1; x++)
    {
      for (int y = 1; y < GridHeight - 1; y++)
      {
        map[x, y] = random.Next(0, 100) < initialWallChance ? TileType.Wall : TileType.Floor;
      }
    }

    for (int i = 0; i < smoothingPasses; i++)
    {
      map = SmoothMap(map);
    }

    List<Vector2Int> mainRegion = KeepLargestFloorRegion(map);
    PlaceRepresentativeDoors(map, mainRegion);
    return map;
  }

  // Applies one neighbour-count smoothing pass to the current wall and floor map.
  TileType[,] SmoothMap(TileType[,] currentMap)
  {
    TileType[,] nextMap = CreateFilledMap(TileType.Wall);

    for (int x = 1; x < GridWidth - 1; x++)
    {
      for (int y = 1; y < GridHeight - 1; y++)
      {
        int surroundingWalls = CountWallNeighbours(currentMap, x, y);

        if (surroundingWalls > 4)
        {
          nextMap[x, y] = TileType.Wall;
        }
        else if (surroundingWalls < 4)
        {
          nextMap[x, y] = TileType.Floor;
        }
        else
        {
          nextMap[x, y] = currentMap[x, y];
        }
      }
    }

    return nextMap;
  }

  // Counts wall or out-of-bounds cells in the eight-cell neighbourhood.
  int CountWallNeighbours(TileType[,] map, int centerX, int centerY)
  {
    int wallCount = 0;

    for (int offsetX = -1; offsetX <= 1; offsetX++)
    {
      for (int offsetY = -1; offsetY <= 1; offsetY++)
      {
        if (offsetX == 0 && offsetY == 0)
        {
          continue;
        }

        int x = centerX + offsetX;
        int y = centerY + offsetY;

        if (!IsInsideMap(x, y) || GetTile(map, x, y) == TileType.Wall)
        {
          wallCount++;
        }
      }
    }

    return wallCount;
  }

  // Retains only the largest connected floor region and supplies a centre fallback when needed.
  List<Vector2Int> KeepLargestFloorRegion(TileType[,] map)
  {
    bool[,] visited = new bool[GridWidth, GridHeight];
    List<List<Vector2Int>> regions = new List<List<Vector2Int>>();

    for (int x = 1; x < GridWidth - 1; x++)
    {
      for (int y = 1; y < GridHeight - 1; y++)
      {
        if (visited[x, y] || GetTile(map, x, y) != TileType.Floor)
        {
          continue;
        }

        List<Vector2Int> region = FloodFillFloorRegion(map, new Vector2Int(x, y), visited);
        regions.Add(region);
      }
    }

    if (regions.Count == 0)
    {
      Vector2Int fallbackCenter = new Vector2Int(GridWidth / 2, GridHeight / 2);
      SetTile(map, fallbackCenter.x, fallbackCenter.y, TileType.Floor);
      return new List<Vector2Int> { fallbackCenter };
    }

    List<Vector2Int> largest = regions[0];
    foreach (List<Vector2Int> region in regions)
    {
      if (region.Count > largest.Count)
      {
        largest = region;
      }
    }

    HashSet<Vector2Int> largestSet = new HashSet<Vector2Int>(largest);
    for (int x = 1; x < GridWidth - 1; x++)
    {
      for (int y = 1; y < GridHeight - 1; y++)
      {
        Vector2Int position = new Vector2Int(x, y);
        if (GetTile(map, x, y) == TileType.Floor && !largestSet.Contains(position))
        {
          map[x, y] = TileType.Wall;
        }
      }
    }

    return largest;
  }

  // Collects one connected floor region using a four-direction breadth-first traversal.
  List<Vector2Int> FloodFillFloorRegion(TileType[,] map, Vector2Int start, bool[,] visited)
  {
    List<Vector2Int> region = new List<Vector2Int>();
    Queue<Vector2Int> open = new Queue<Vector2Int>();
    open.Enqueue(start);
    visited[start.x, start.y] = true;

    Vector2Int[] directions =
    {
      Vector2Int.up,
      Vector2Int.down,
      Vector2Int.left,
      Vector2Int.right
    };

    while (open.Count > 0)
    {
      Vector2Int current = open.Dequeue();
      region.Add(current);

      foreach (Vector2Int direction in directions)
      {
        Vector2Int next = current + direction;
        if (!IsInsideMap(next.x, next.y) || visited[next.x, next.y] || GetTile(map, next.x, next.y) != TileType.Floor)
        {
          continue;
        }

        visited[next.x, next.y] = true;
        open.Enqueue(next);
      }
    }

    return region;
  }

  // Marks the horizontal extremes of the main region as representative door cells.
  void PlaceRepresentativeDoors(TileType[,] map, List<Vector2Int> mainRegion)
  {
    if (mainRegion == null || mainRegion.Count == 0)
    {
      return;
    }

    Vector2Int leftMost = mainRegion[0];
    Vector2Int rightMost = mainRegion[0];

    foreach (Vector2Int cell in mainRegion)
    {
      if (cell.x < leftMost.x)
      {
        leftMost = cell;
      }

      if (cell.x > rightMost.x)
      {
        rightMost = cell;
      }
    }

    map[leftMost.x, leftMost.y] = TileType.Door;
    map[rightMost.x, rightMost.y] = TileType.Door;
  }
}
