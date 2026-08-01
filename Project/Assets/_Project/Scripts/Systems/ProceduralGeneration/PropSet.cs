using UnityEngine;

/// <summary>
/// Sparse/probabilistic prop library the prop placement pass reads from. Unlike DungeonTileSet (exactly
/// one prefab per grid cell), props are optional - each eligible tile independently rolls a chance to
/// spawn one, seeded so the same dungeon seed always produces the same result.
/// </summary>
[CreateAssetMenu(fileName = "PropSet", menuName = "Dungeon/Prop Set")]
public class PropSet : ScriptableObject
{
  [Header("Wall Props")]
  public GameObject[] wallProps;
  [Range(0f, 1f)] public float wallPropChance = 0.15f;

  [Tooltip("Max random slide left/right along the wall.")]
  [Range(0f, 0.5f)] public float wallPositionJitter = 0.3f;

  [Header("Floor Props")]
  public GameObject[] floorProps;
  [Range(0f, 1f)] public float floorPropChance = 0.1f;

  [Tooltip("Max random offset on each of the local X/Z axes")]
  [Range(0f, 0.5f)] public float floorPositionJitter = 0.3f;

  [Tooltip("Max random extra rotation (in degrees, either direction).")]
  [Range(0f, 180f)] public float floorRotationJitterDegrees = 20f;
}
