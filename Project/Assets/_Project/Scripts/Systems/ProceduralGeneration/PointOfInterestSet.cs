using UnityEngine;

[CreateAssetMenu(fileName = "PointOfInterestSet", menuName = "Dungeon/Point Of Interest Set")]
public class PointOfInterestSet : ScriptableObject
{
  [Header("Spawn Room")]
  public GameObject spawnPortalPrefab;
  public Color spawnPortalColour = new Color(0.2f, 0.6f, 1f, 1f);

  [Header("Loot Rooms")]
  public GameObject[] lootPrefabs;
  public Color lootColour = new Color(1f, 0.85f, 0f, 1f);
}
