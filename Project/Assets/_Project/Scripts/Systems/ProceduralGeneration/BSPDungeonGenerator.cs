using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 2D proof-of-concept BSP dungeon generator for comparing room-based procedural generation.
/// Grey tiles are floor, black tiles are walls, and red tiles indicate door positions.
/// </summary>
public class BSPDungeonGenerator : DungeonGridGenerator2D
{
  [Header("BSP Settings")]
  [SerializeField, Min(1)] int maxDepth = 4;
  [SerializeField, Min(8)] int minLeafSize = 12;
  [SerializeField, Min(3)] int minimumRoomSize = 5;
  [SerializeField, Min(0)] int minCorridorLength = 2;

  int corridorWidth = 1;

  // Two sibling rooms can each be placed as close as RoomPadding cells from their shared leaf
  // boundary, so the worst-case gap between them is 2 * RoomPadding. Doors occupy the first and
  // last cell of that gap, leaving (2 * RoomPadding) - 2 cells of pure corridor floor - deriving
  // RoomPadding from minCorridorLength guarantees that stays >= minCorridorLength.
  int RoomPadding => Mathf.Max(1, Mathf.CeilToInt((minCorridorLength + 2) / 2f));

  protected override void OnValidate()
  {
    base.OnValidate();
    maxDepth = Mathf.Max(1, maxDepth);
    minLeafSize = Mathf.Max(8, minLeafSize);
    minimumRoomSize = Mathf.Max(3, minimumRoomSize);
    minCorridorLength = Mathf.Max(0, minCorridorLength);
  }

  protected override TileType[,] BuildMap(int seed)
  {
    System.Random random = new System.Random(seed);
    TileType[,] map = CreateFilledMap(TileType.Wall);

    RectInt rootArea = new RectInt(1, 1, GridWidth - 2, GridHeight - 2);
    BspNode root = SplitRecursively(rootArea, 0, random);

    CreateRooms(root, random, map);
    ConnectSiblingRooms(root, random, map);

    return map;
  }

  BspNode SplitRecursively(RectInt area, int depth, System.Random random)
  {
    BspNode node = new BspNode(area);

    if (depth >= maxDepth)
    {
      return node;
    }

    bool canSplitHorizontally = area.height >= minLeafSize * 2;
    bool canSplitVertically = area.width >= minLeafSize * 2;

    if (!canSplitHorizontally && !canSplitVertically)
    {
      return node;
    }

    bool splitHorizontally = ChooseSplitOrientation(area, canSplitHorizontally, canSplitVertically, random);

    if (splitHorizontally)
    {
      int splitY = random.Next(minLeafSize, area.height - minLeafSize + 1);
      RectInt bottom = new RectInt(area.xMin, area.yMin, area.width, splitY);
      RectInt top = new RectInt(area.xMin, area.yMin + splitY, area.width, area.height - splitY);

      node.Left = SplitRecursively(bottom, depth + 1, random);
      node.Right = SplitRecursively(top, depth + 1, random);
    }
    else
    {
      int splitX = random.Next(minLeafSize, area.width - minLeafSize + 1);
      RectInt left = new RectInt(area.xMin, area.yMin, splitX, area.height);
      RectInt right = new RectInt(area.xMin + splitX, area.yMin, area.width - splitX, area.height);

      node.Left = SplitRecursively(left, depth + 1, random);
      node.Right = SplitRecursively(right, depth + 1, random);
    }

    return node;
  }

  bool ChooseSplitOrientation(RectInt area, bool canSplitHorizontally, bool canSplitVertically, System.Random random)
  {
    if (!canSplitHorizontally)
    {
      return false;
    }

    if (!canSplitVertically)
    {
      return true;
    }

    float ratio = (float)area.width / area.height;
    if (ratio >= 1.25f)
    {
      return false;
    }

    if (ratio <= 0.8f)
    {
      return true;
    }

    return random.NextDouble() > 0.5;
  }

  void CreateRooms(BspNode node, System.Random random, TileType[,] map)
  {
    if (node == null)
    {
      return;
    }

    if (!node.IsLeaf)
    {
      CreateRooms(node.Left, random, map);
      CreateRooms(node.Right, random, map);
      return;
    }

    int maxRoomWidth = Mathf.Max(minimumRoomSize, node.Area.width - (RoomPadding * 2));
    int maxRoomHeight = Mathf.Max(minimumRoomSize, node.Area.height - (RoomPadding * 2));

    int roomWidth = random.Next(minimumRoomSize, maxRoomWidth + 1);
    int roomHeight = random.Next(minimumRoomSize, maxRoomHeight + 1);

    int roomXMin = node.Area.xMin + RoomPadding;
    int roomYMin = node.Area.yMin + RoomPadding;
    int roomXMax = Mathf.Max(roomXMin, node.Area.xMax - RoomPadding - roomWidth);
    int roomYMax = Mathf.Max(roomYMin, node.Area.yMax - RoomPadding - roomHeight);

    int roomX = random.Next(roomXMin, roomXMax + 1);
    int roomY = random.Next(roomYMin, roomYMax + 1);

    node.Room = new RectInt(roomX, roomY, roomWidth, roomHeight);
    node.HasRoom = true;

    for (int x = node.Room.xMin; x < node.Room.xMax; x++)
    {
      for (int y = node.Room.yMin; y < node.Room.yMax; y++)
      {
        map[x, y] = TileType.Floor;
      }
    }

    MarkCorner(map, node.Room.xMin - 1, node.Room.yMin - 1);
    MarkCorner(map, node.Room.xMax, node.Room.yMin - 1);
    MarkCorner(map, node.Room.xMin - 1, node.Room.yMax);
    MarkCorner(map, node.Room.xMax, node.Room.yMax);
  }

  void MarkCorner(TileType[,] map, int x, int y)
  {
    if (IsInsideMap(x, y) && GetTile(map, x, y) == TileType.Wall)
    {
      SetTile(map, x, y, TileType.Corner);
    }
  }

  void ConnectSiblingRooms(BspNode node, System.Random random, TileType[,] map)
  {
    if (node == null || node.IsLeaf)
    {
      return;
    }

    ConnectSiblingRooms(node.Left, random, map);
    ConnectSiblingRooms(node.Right, random, map);

    if (!TryFindClosestRoomPair(node.Left, node.Right, out RectInt leftRoom, out RectInt rightRoom))
    {
      return;
    }

    Vector2Int start = GetRoomCenter(leftRoom);
    Vector2Int end = GetRoomCenter(rightRoom);

    bool useHorizontalFirst = random.NextDouble() > 0.5;
    List<Vector2Int> corridorPath = useHorizontalFirst
      ? BuildHorizontalThenVerticalPath(start, end)
      : BuildVerticalThenHorizontalPath(start, end);

    CarveCorridorPath(corridorPath, map);
    TryPlaceDoorAtCorridorBoundary(leftRoom, corridorPath, true, map);
    TryPlaceDoorAtCorridorBoundary(rightRoom, corridorPath, false, map);
  }

  bool TryFindClosestRoomPair(BspNode leftNode, BspNode rightNode, out RectInt leftRoom, out RectInt rightRoom)
  {
    List<RectInt> leftRooms = new List<RectInt>();
    List<RectInt> rightRooms = new List<RectInt>();
    CollectRooms(leftNode, leftRooms);
    CollectRooms(rightNode, rightRooms);

    leftRoom = default;
    rightRoom = default;

    if (leftRooms.Count == 0 || rightRooms.Count == 0)
    {
      return false;
    }

    int bestDistance = int.MaxValue;

    foreach (RectInt first in leftRooms)
    {
      Vector2Int firstCenter = GetRoomCenter(first);

      foreach (RectInt second in rightRooms)
      {
        Vector2Int secondCenter = GetRoomCenter(second);
        int distance = Mathf.Abs(firstCenter.x - secondCenter.x) + Mathf.Abs(firstCenter.y - secondCenter.y);

        if (distance >= bestDistance)
        {
          continue;
        }

        bestDistance = distance;
        leftRoom = first;
        rightRoom = second;
      }
    }

    return true;
  }

  void CollectRooms(BspNode node, List<RectInt> rooms)
  {
    if (node == null)
    {
      return;
    }

    if (node.HasRoom)
    {
      rooms.Add(node.Room);
    }

    CollectRooms(node.Left, rooms);
    CollectRooms(node.Right, rooms);
  }

  List<Vector2Int> BuildHorizontalThenVerticalPath(Vector2Int start, Vector2Int end)
  {
    List<Vector2Int> path = new List<Vector2Int>();
    AddLineToPath(path, start, new Vector2Int(end.x, start.y));
    AddLineToPath(path, new Vector2Int(end.x, start.y), end);
    return path;
  }

  List<Vector2Int> BuildVerticalThenHorizontalPath(Vector2Int start, Vector2Int end)
  {
    List<Vector2Int> path = new List<Vector2Int>();
    AddLineToPath(path, start, new Vector2Int(start.x, end.y));
    AddLineToPath(path, new Vector2Int(start.x, end.y), end);
    return path;
  }

  void AddLineToPath(List<Vector2Int> path, Vector2Int from, Vector2Int to)
  {
    Vector2Int current = from;
    Vector2Int step = new Vector2Int(Math.Sign(to.x - from.x), Math.Sign(to.y - from.y));

    if (path.Count == 0 || path[path.Count - 1] != current)
    {
      path.Add(current);
    }

    while (current != to)
    {
      current += step;
      if (path[path.Count - 1] != current)
      {
        path.Add(current);
      }
    }
  }

  void CarveCorridorPath(List<Vector2Int> corridorPath, TileType[,] map)
  {
    foreach (Vector2Int cell in corridorPath)
    {
      CarveBrush(cell, map);
    }
  }

  void CarveBrush(Vector2Int center, TileType[,] map)
  {
    int negativeOffset = corridorWidth / 2;
    int positiveOffset = corridorWidth - negativeOffset;

    for (int x = center.x - negativeOffset; x < center.x + positiveOffset; x++)
    {
      for (int y = center.y - negativeOffset; y < center.y + positiveOffset; y++)
      {
        if (IsInsideMap(x, y))
        {
          map[x, y] = TileType.Floor;
        }
      }
    }
  }

  void TryPlaceDoorAtCorridorBoundary(RectInt room, List<Vector2Int> corridorPath, bool fromStart, TileType[,] map)
  {
    if (corridorPath == null || corridorPath.Count < 2)
    {
      return;
    }

    if (fromStart)
    {
      for (int i = 1; i < corridorPath.Count; i++)
      {
        Vector2Int previous = corridorPath[i - 1];
        Vector2Int current = corridorPath[i];

        if (IsInsideRoom(room, previous) && !IsInsideRoom(room, current))
        {
          PlaceDoorIfConnected(map, previous, current);
          return;
        }
      }

      return;
    }

    for (int i = corridorPath.Count - 2; i >= 0; i--)
    {
      Vector2Int current = corridorPath[i];
      Vector2Int next = corridorPath[i + 1];

      if (!IsInsideRoom(room, current) && IsInsideRoom(room, next))
      {
        PlaceDoorIfConnected(map, next, current);
        return;
      }
    }
  }

  void PlaceDoorIfConnected(TileType[,] map, Vector2Int roomCell, Vector2Int corridorCell)
  {
    if (!IsInsideMap(roomCell.x, roomCell.y) || !IsInsideMap(corridorCell.x, corridorCell.y))
    {
      return;
    }

    TileType roomTile = map[roomCell.x, roomCell.y];
    if (roomTile == TileType.Floor || roomTile == TileType.Door)
    {
      map[corridorCell.x, corridorCell.y] = TileType.Door;
    }
  }

  bool IsInsideRoom(RectInt room, Vector2Int cell)
  {
    return cell.x >= room.xMin && cell.x < room.xMax && cell.y >= room.yMin && cell.y < room.yMax;
  }

  Vector2Int GetRoomCenter(RectInt room)
  {
    return new Vector2Int(room.xMin + (room.width / 2), room.yMin + (room.height / 2));
  }

  sealed class BspNode
  {
    public readonly RectInt Area;
    public BspNode Left;
    public BspNode Right;
    public RectInt Room;
    public bool HasRoom;

    public bool IsLeaf => Left == null && Right == null;

    public BspNode(RectInt area)
    {
      Area = area;
    }
  }
}
