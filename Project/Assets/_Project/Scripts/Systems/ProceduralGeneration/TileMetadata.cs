/// <summary>
/// Per-tile data describing what a tile is and what surrounds it, derived from a TileType[,] grid.
/// Walls/Doorways/Floors are flags rather than a single tag so combinations fall out of the data
/// instead of needing their own enum case. Floors carries both cardinal (North/East/South/West) and
/// diagonal (NorthEast/etc.) bits: a Wall tile with a cardinal Floors bit is a straight wall facing
/// that direction.
/// </summary>
public struct TileMetadata
{
  public TileType Type;
  public Direction Walls;
  public Direction Doorways;
  public Direction Floors;

  /// <summary>
  /// For a Door tile only: which cardinal direction leads toward the wider (room) side rather than
  /// the 1-tile-wide corridor side, used to orient an asymmetric door prefab consistently.
  /// </summary>
  public Direction DoorRoomSide;
}
