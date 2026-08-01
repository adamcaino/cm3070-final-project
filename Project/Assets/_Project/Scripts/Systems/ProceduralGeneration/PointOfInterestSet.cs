using UnityEngine;

/// <summary>
/// Prefab/appearance library the POI placement pass reads from. Spawn and boss each get a single portal
/// prefab (one per room, unlike a variant array) - the boss room's portal is what marks it as a boss
/// room at all; actual boss spawning/UI/behaviour reads from it later and isn't handled here. Loot rooms
/// roll between a normal chest (immediate, smaller reward) and a cursed one (locks the room and spawns
/// enemy waves for a bigger reward) - that lock/wave logic isn't handled here either, this only decides
/// which chest variant gets placed. Leave a prefab/array empty to fall back to a placeholder primitive
/// of the matching color, same convention as DungeonTileSet/PropSet.
/// </summary>
[CreateAssetMenu(fileName = "PointOfInterestSet", menuName = "Dungeon/Point Of Interest Set")]
public class PointOfInterestSet : ScriptableObject
{
  [Header("Spawn Room")]
  [Tooltip("Instantiated in the spawn room, with the player placed at the same spot - e.g. a portal the player steps out of, that they'll need to return to once the boss is dead to win the level.")]
  public GameObject spawnPortalPrefab;
  public Color spawnPortalColor = new Color(0.2f, 0.6f, 1f, 1f);

  [Header("Boss Room")]
  [Tooltip("The boss room's portal, not the boss itself - placed at room center, facing the corridor that leads into the room.")]
  public GameObject bossPortalPrefab;
  public Color bossPortalColor = new Color(0.6f, 0f, 0f, 1f);

  [Header("Loot Rooms")]
  public GameObject[] normalChestPrefabs;
  public Color normalChestColor = new Color(1f, 0.85f, 0f, 1f);

  public GameObject[] cursedChestPrefabs;
  public Color cursedChestColor = new Color(0.45f, 0f, 0.55f, 1f);

  [Range(0f, 1f)] public float cursedChestChance = 0.25f;
}
