// Identifies the structural role of a cell in the generated dungeon grid.
public enum TileType
{
  // Represents unused space outside the visible dungeon.
  Empty,
  // Represents traversable dungeon space.
  Floor,
  // Represents solid dungeon geometry.
  Wall,
  // Represents a traversable connection between a room and corridor.
  Door
}
