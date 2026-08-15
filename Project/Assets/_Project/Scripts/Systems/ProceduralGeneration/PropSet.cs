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
}
