using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

// Sweeps the BSP generator across many seeds to measure room overlap, connectivity, and door
// validity beyond the small set of seeds inspected manually during development.
public class BSPDungeonGeneratorReliabilityTests
{
  const int SeedCount = 200;
  const int FirstSeed = 1;

  static readonly Vector2Int[] CardinalOffsets =
  {
    new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0)
  };

  [Test]
  public void GeneratesConnectedNonOverlappingDungeonsAcrossManySeeds()
  {
    GameObject host = new GameObject(nameof(BSPDungeonGeneratorReliabilityTests));
    BSPDungeonGenerator generator = host.AddComponent<BSPDungeonGenerator>();

    int missingRoleFailures = 0;
    int overlapFailures = 0;
    int connectivityFailures = 0;
    int invalidDoorCells = 0;

    try
    {
      for (int seed = FirstSeed; seed < FirstSeed + SeedCount; seed++)
      {
        generator.Generate(seed);

        IReadOnlyList<DungeonRoomInfo> rooms = generator.LastRooms;
        TileMetadata[,] metadata = generator.LastMetadata;

        if (!HasRequiredRoles(rooms))
        {
          missingRoleFailures++;
        }

        if (HasOverlappingRooms(rooms))
        {
          overlapFailures++;
        }

        if (!AllRoomsReachableFromSpawn(rooms, metadata))
        {
          connectivityFailures++;
        }

        invalidDoorCells += CountInvalidDoors(metadata);
      }
    }
    finally
    {
      Object.DestroyImmediate(host);
    }

    Debug.Log(
      $"BSP reliability sweep over {SeedCount} seeds (starting at {FirstSeed}): " +
      $"{missingRoleFailures} seeds missing a spawn or boss room, " +
      $"{overlapFailures} seeds with overlapping rooms, " +
      $"{connectivityFailures} seeds with an unreachable room, " +
      $"{invalidDoorCells} invalid door cells in total.");

    Assert.AreEqual(0, missingRoleFailures, $"{missingRoleFailures} of {SeedCount} seeds did not assign both a spawn and a boss room.");
    Assert.AreEqual(0, overlapFailures, $"{overlapFailures} of {SeedCount} seeds produced overlapping room bounds.");
    Assert.AreEqual(0, connectivityFailures, $"{connectivityFailures} of {SeedCount} seeds produced a room unreachable from spawn.");
    Assert.AreEqual(0, invalidDoorCells, $"{invalidDoorCells} door cells across {SeedCount} seeds did not open onto two traversable sides.");
  }

  // Confirms every generated map assigns exactly the roles later placement passes depend on.
  static bool HasRequiredRoles(IReadOnlyList<DungeonRoomInfo> rooms)
  {
    bool hasSpawn = false;
    bool hasBoss = false;

    foreach (DungeonRoomInfo room in rooms)
    {
      if (room.Role == RoomRole.Spawn)
      {
        hasSpawn = true;
      }

      if (room.Role == RoomRole.Boss)
      {
        hasBoss = true;
      }
    }

    return hasSpawn && hasBoss;
  }

  // Detects any pair of rooms whose floor rectangles intersect.
  static bool HasOverlappingRooms(IReadOnlyList<DungeonRoomInfo> rooms)
  {
    for (int i = 0; i < rooms.Count; i++)
    {
      for (int j = i + 1; j < rooms.Count; j++)
      {
        if (rooms[i].Bounds.Overlaps(rooms[j].Bounds))
        {
          return true;
        }
      }
    }

    return false;
  }

  // Flood-fills from the spawn room and checks that every room has at least one reachable cell.
  static bool AllRoomsReachableFromSpawn(IReadOnlyList<DungeonRoomInfo> rooms, TileMetadata[,] metadata)
  {
    Vector2Int? spawnCell = null;

    foreach (DungeonRoomInfo room in rooms)
    {
      if (room.Role == RoomRole.Spawn)
      {
        spawnCell = room.Center;
        break;
      }
    }

    if (!spawnCell.HasValue)
    {
      return false;
    }

    HashSet<Vector2Int> reachable = FloodFillTraversable(metadata, spawnCell.Value);

    foreach (DungeonRoomInfo room in rooms)
    {
      if (!AnyCellReachable(room.Bounds, reachable))
      {
        return false;
      }
    }

    return true;
  }

  static bool AnyCellReachable(RectInt bounds, HashSet<Vector2Int> reachable)
  {
    for (int x = bounds.xMin; x < bounds.xMax; x++)
    {
      for (int y = bounds.yMin; y < bounds.yMax; y++)
      {
        if (reachable.Contains(new Vector2Int(x, y)))
        {
          return true;
        }
      }
    }

    return false;
  }

  // Breadth-first search over floor and door cells starting from the given cell.
  static HashSet<Vector2Int> FloodFillTraversable(TileMetadata[,] metadata, Vector2Int start)
  {
    int width = metadata.GetLength(0);
    int height = metadata.GetLength(1);
    HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

    if (!IsTraversable(metadata, start, width, height))
    {
      return visited;
    }

    Queue<Vector2Int> frontier = new Queue<Vector2Int>();
    visited.Add(start);
    frontier.Enqueue(start);

    while (frontier.Count > 0)
    {
      Vector2Int current = frontier.Dequeue();

      foreach (Vector2Int offset in CardinalOffsets)
      {
        Vector2Int next = current + offset;

        if (visited.Contains(next) || !IsTraversable(metadata, next, width, height))
        {
          continue;
        }

        visited.Add(next);
        frontier.Enqueue(next);
      }
    }

    return visited;
  }

  static bool IsTraversable(TileMetadata[,] metadata, Vector2Int cell, int width, int height)
  {
    if (cell.x < 0 || cell.y < 0 || cell.x >= width || cell.y >= height)
    {
      return false;
    }

    TileType type = metadata[cell.x, cell.y].Type;
    return type == TileType.Floor || type == TileType.Door;
  }

  // Counts door cells that do not open onto at least two traversable cardinal sides, which
  // would indicate a door placed without a genuine room-to-corridor transition either side.
  static int CountInvalidDoors(TileMetadata[,] metadata)
  {
    int width = metadata.GetLength(0);
    int height = metadata.GetLength(1);
    int invalidCount = 0;

    for (int x = 0; x < width; x++)
    {
      for (int y = 0; y < height; y++)
      {
        if (metadata[x, y].Type != TileType.Door)
        {
          continue;
        }

        Direction traversableSides = metadata[x, y].Floors | metadata[x, y].Doorways;
        if (CountCardinalFlags(traversableSides) < 2)
        {
          invalidCount++;
        }
      }
    }

    return invalidCount;
  }

  static int CountCardinalFlags(Direction direction)
  {
    int count = 0;

    if ((direction & Direction.North) != 0) count++;
    if ((direction & Direction.East) != 0) count++;
    if ((direction & Direction.South) != 0) count++;
    if ((direction & Direction.West) != 0) count++;

    return count;
  }
}
