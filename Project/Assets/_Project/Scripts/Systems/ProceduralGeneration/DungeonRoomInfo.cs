using UnityEngine;

/// <summary>
/// A single BSP leaf room surviving generation, with the grid-space data a POI placement pass needs -
/// the room's footprint (for picking a spawn point anywhere inside it) and its center (for single-point
/// placements like a boss or loot pile).
/// </summary>
public struct DungeonRoomInfo
{
  public int RoomId;
  public RectInt Bounds;
  public Vector2Int Center;
  public RoomRole Role;
}
