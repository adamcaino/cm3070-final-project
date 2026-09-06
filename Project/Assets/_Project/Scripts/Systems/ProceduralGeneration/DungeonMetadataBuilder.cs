// Converts a tile grid into neighbour metadata used by rendering and placement passes.
// Isolated walls become empty cells, while cardinal and diagonal neighbours remain available.
public static class DungeonMetadataBuilder
{
  struct NeighborOffset
  {
    public readonly Direction Direction;
    public readonly int Dx;
    public readonly int Dy;

    public NeighborOffset(Direction direction, int dx, int dy)
    {
      Direction = direction;
      Dx = dx;
      Dy = dy;
    }
  }

  static readonly NeighborOffset[] CardinalOffsets =
  {
    new NeighborOffset(Direction.North, 0, 1),
    new NeighborOffset(Direction.East, 1, 0),
    new NeighborOffset(Direction.South, 0, -1),
    new NeighborOffset(Direction.West, -1, 0)
  };

  static readonly NeighborOffset[] DiagonalOffsets =
  {
    new NeighborOffset(Direction.NorthEast, 1, 1),
    new NeighborOffset(Direction.SouthEast, 1, -1),
    new NeighborOffset(Direction.SouthWest, -1, -1),
    new NeighborOffset(Direction.NorthWest, -1, 1)
  };

  static readonly (int Dx, int Dy)[] EightWayOffsets =
  {
    (0, 1), (1, 1), (1, 0), (1, -1),
    (0, -1), (-1, -1), (-1, 0), (-1, 1)
  };

  // Builds neighbour metadata for every cell in the generated tile map.
  public static TileMetadata[,] Build(TileType[,] map)
  {
    int width = map.GetLength(0);
    int height = map.GetLength(1);
    TileMetadata[,] metadata = new TileMetadata[width, height];

    for (int x = 0; x < width; x++)
    {
      for (int y = 0; y < height; y++)
      {
        metadata[x, y] = BuildTileMetadata(map, x, y, width, height);
      }
    }

    return metadata;
  }

  // Classifies one cell and records its cardinal, diagonal, wall, floor, and doorway neighbours.
  static TileMetadata BuildTileMetadata(TileType[,] map, int x, int y, int width, int height)
  {
    TileType type = map[x, y];

    if (type == TileType.Wall && !IsNearFloor(map, x, y, width, height))
    {
      type = TileType.Empty;
    }

    TileMetadata tile = new TileMetadata { Type = type };

    if (type == TileType.Empty)
    {
      return tile;
    }

    foreach (NeighborOffset offset in CardinalOffsets)
    {
      TileType neighborType = GetNeighborTileType(map, x + offset.Dx, y + offset.Dy, width, height);

      if (neighborType == TileType.Wall)
      {
        tile.Walls |= offset.Direction;
      }
      else if (neighborType == TileType.Door)
      {
        tile.Doorways |= offset.Direction;
      }
      else if (neighborType == TileType.Floor)
      {
        tile.Floors |= offset.Direction;
      }
    }

    foreach (NeighborOffset offset in DiagonalOffsets)
    {
      if (GetNeighborTileType(map, x + offset.Dx, y + offset.Dy, width, height) == TileType.Floor)
      {
        tile.Floors |= offset.Direction;
      }
    }

    if (type == TileType.Door)
    {
      tile.DoorRoomSide = FindDoorRoomSide(map, x, y, tile.Floors, width, height);
    }

    return tile;
  }

  // Identifies the side of a door that opens into the wider room rather than the corridor.
  static Direction FindDoorRoomSide(TileType[,] map, int x, int y, Direction floors, int width, int height)
  {
    foreach (NeighborOffset offset in CardinalOffsets)
    {
      if ((floors & offset.Direction) == Direction.None)
      {
        continue;
      }

      int neighborX = x + offset.Dx;
      int neighborY = y + offset.Dy;

      foreach ((int perpendicularDx, int perpendicularDy) in GetPerpendicularOffsets(offset.Direction))
      {
        TileType perpendicularType = GetNeighborTileType(map, neighborX + perpendicularDx, neighborY + perpendicularDy, width, height);

        if (perpendicularType == TileType.Floor)
        {
          return offset.Direction;
        }
      }
    }

    return Direction.None;
  }

  // Returns grid offsets perpendicular to a cardinal direction.
  static (int Dx, int Dy)[] GetPerpendicularOffsets(Direction cardinalDirection)
  {
    if (cardinalDirection == Direction.North || cardinalDirection == Direction.South)
    {
      return new[] { (1, 0), (-1, 0) };
    }

    return new[] { (0, 1), (0, -1) };
  }

  // Tests the eight neighbouring cells for floor so isolated walls can become empty space.
  static bool IsNearFloor(TileType[,] map, int x, int y, int width, int height)
  {
    foreach ((int dx, int dy) in EightWayOffsets)
    {
      if (GetNeighborTileType(map, x + dx, y + dy, width, height) == TileType.Floor)
      {
        return true;
      }
    }

    return false;
  }

  // Reads a neighbour and treats coordinates outside the grid as walls.
  static TileType GetNeighborTileType(TileType[,] map, int x, int y, int width, int height)
  {
    if (x < 0 || y < 0 || x >= width || y >= height)
    {
      return TileType.Wall;
    }

    return map[x, y];
  }
}
