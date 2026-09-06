using UnityEngine;

// Stores prefab variants and placeholder colours used by the 3D tile placement pass.
// Variant selection is seeded by the generated dungeon seed.
[CreateAssetMenu(fileName = "DungeonTileSet", menuName = "Dungeon/Tile Set")]
public class DungeonTileSet : ScriptableObject
{
  [Header("Prefab Variants")]
  public GameObject[] floorPrefabs;
  public GameObject[] wallPrefabs;
  public GameObject[] doorPrefabs;

  // [Header("Pivot Offsets (local space, corrects prefab authoring - applied along the tile's own rotation)")]
  // public Vector3 floorPivotOffset;
  // public Vector3 wallPivotOffset;
  // public Vector3 doorPivotOffset;

  [Header("Placeholder Colors")]
  public Color floorColor = new Color(0.5f, 0.5f, 0.5f, 1f);
  public Color wallColor = new Color(0.2f, 0.2f, 0.2f, 1f);
  public Color doorColor = Color.red;
}
