using UnityEngine;

// Stores the grid-space bounds, role, centre, and graph distances for one generated BSP room.
public struct DungeonRoomInfo
{
  // Stable index assigned when the room is created by the BSP generator.
  public int RoomId;
  // Grid-space rectangle occupied by the room's floor.
  public RectInt Bounds;
  // Grid-space centre used for single-point placements.
  public Vector2Int Center;
  // Placement role assigned after the room graph is analysed.
  public RoomRole Role;
  // Number of room-graph edges from the spawn room.
  public int DistanceFromSpawn;
  // Number of room-graph edges from the boss room.
  public int DistanceFromBoss;
  // Room-graph distance between the selected spawn and boss rooms.
  public int SpawnToBossDistance;
}
