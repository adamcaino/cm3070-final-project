// Stores a tile type and the neighbouring walls, doors, and floors derived from a tile grid.
public struct TileMetadata
{
  public TileType Type;
  public Direction Walls;
  public Direction Doorways;
  public Direction Floors;

  // For a door, identifies the cardinal direction toward the wider room side.
  public Direction DoorRoomSide;
}
