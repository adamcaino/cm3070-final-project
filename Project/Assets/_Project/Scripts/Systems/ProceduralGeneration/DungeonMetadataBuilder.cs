/// <summary>
/// Converts a raw TileType[,] grid into a TileMetadata[,] grid by inspecting each tile's neighbors.
/// Works on any generator's output (BSP, etc.) since it only depends on TileType, not room geometry.
/// Wall tiles with no floor tile within one step (including diagonals) are treated as void/empty rock
/// rather than real walls, since nothing will ever render or need to be seen there in a top-down view.
/// </summary>
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

  static readonly (int Dx, int Dy)[] EightWayOffsets =
  {
    (0, 1), (1, 1), (1, 0), (1, -1),
    (0, -1), (-1, -1), (-1, 0), (-1, 1)
  };

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

    return tile;
  }

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

  static TileType GetNeighborTileType(TileType[,] map, int x, int y, int width, int height)
  {
    if (x < 0 || y < 0 || x >= width || y >= height)
    {
      return TileType.Wall;
    }

    return map[x, y];
  }
}
