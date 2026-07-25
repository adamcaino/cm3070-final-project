/// <summary>
/// Per-tile data describing what a tile is and what surrounds it, derived from a TileType[,] grid.
/// Walls/Doorways/Floors are flags rather than a single tag so combinations (e.g. two adjacent walls
/// forming a corner) fall out of the data instead of needing their own enum case. For a Wall tile,
/// Floors indicates which cardinal direction(s) it faces open floor, used to orient a decorated wall
/// prefab's front face.
/// </summary>
public struct TileMetadata
{
  public TileType Type;
  public Direction Walls;
  public Direction Doorways;
  public Direction Floors;
}
