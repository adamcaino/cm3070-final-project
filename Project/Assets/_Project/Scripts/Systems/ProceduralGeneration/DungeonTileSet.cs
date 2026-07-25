using UnityEngine;

/// <summary>
/// Prefab/appearance library the 3D placement pass reads from. Leave a prefab field empty to fall
/// back to a placeholder primitive of the matching color until the real art exists.
/// </summary>
[CreateAssetMenu(fileName = "DungeonTileSet", menuName = "Dungeon/Tile Set")]
public class DungeonTileSet : ScriptableObject
{
  [Header("Prefabs (leave empty to use placeholder primitives)")]
  public GameObject floorPrefab;
  public GameObject wallPrefab;
  public GameObject doorPrefab;

  [Header("Placeholder Colors")]
  public Color floorColor = new Color(0.5f, 0.5f, 0.5f, 1f);
  public Color wallColor = new Color(0.2f, 0.2f, 0.2f, 1f);
  public Color doorColor = Color.red;
}
