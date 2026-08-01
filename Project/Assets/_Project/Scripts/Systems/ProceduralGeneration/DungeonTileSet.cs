using UnityEngine;

/// <summary>
/// Prefab/appearance library the 3D placement pass reads from. Each tile type holds an array of
/// variants - the placer picks one at random per tile (seeded, so it's reproducible per dungeon seed).
/// Leave an array empty to fall back to a placeholder primitive of the matching color until real art
/// exists. All variants of a type are assumed to share the same footprint/pivot convention, since the
/// pivot offset below is applied uniformly regardless of which variant gets picked.
/// </summary>
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
