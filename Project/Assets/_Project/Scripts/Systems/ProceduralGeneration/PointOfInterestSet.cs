using UnityEngine;

[CreateAssetMenu(fileName = "PointOfInterestSet", menuName = "Dungeon/Point Of Interest Set")]
// Stores portal and loot prefab choices used by the point-of-interest placement pass.
public class PointOfInterestSet : ScriptableObject
{
  [Header("Spawn Room")]
  // Optional portal prefab placed in the generated spawn room.
  public GameObject spawnPortalPrefab;
  // Placeholder colour used when no portal prefab is assigned.
  public Color spawnPortalColour = new Color(0.2f, 0.6f, 1f, 1f);

  [Header("Loot Rooms")]
  // Loot prefab variants selected for generated loot rooms.
  public GameObject[] lootPrefabs;
  // Placeholder colour used when no loot prefab is assigned.
  public Color lootColour = new Color(1f, 0.85f, 0f, 1f);
}
