using System.Collections.Generic;
using UnityEngine;

// Places seeded wall props on eligible metadata cells after 3D tiles have been generated.
// Adjacent occupied cells are skipped to keep props separated.
public class DungeonPropPlacer3D : MonoBehaviour
{
  const Direction CardinalDirections = Direction.North | Direction.East | Direction.South | Direction.West;

  [SerializeField] DungeonGridGenerator2D sourceGenerator;
  [SerializeField] DungeonTilePlacer3D tilePlacer;
  [SerializeField] PropSet propSet;

  Transform generatedRoot;
  System.Random propRandom;
  readonly HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();

  [ContextMenu("Generate")]
  // Reads generated metadata, resets prior props, and performs the seeded wall-prop pass.
  public void Generate()
  {
    if (sourceGenerator == null || tilePlacer == null || propSet == null)
    {
      Debug.LogWarning($"{nameof(DungeonPropPlacer3D)} is missing a source generator, tile placer, or prop set.");
      return;
    }

    TileMetadata[,] metadata = sourceGenerator.LastMetadata;
    if (metadata == null)
    {
      Debug.LogWarning($"{nameof(DungeonPropPlacer3D)} found no generated metadata - generate the 2D map first.");
      return;
    }

    Clear();
    EnsureGeneratedRoot();

    propRandom = new System.Random(sourceGenerator.LastUsedSeed);
    occupiedCells.Clear();

    float tileSize = tilePlacer.TileSize;
    int width = metadata.GetLength(0);
    int height = metadata.GetLength(1);
    float xOffset = (width - 1) * tileSize * 0.5f;
    float zOffset = (height - 1) * tileSize * 0.5f;

    for (int x = 0; x < width; x++)
    {
      for (int y = 0; y < height; y++)
      {
        TryPlaceProp(metadata[x, y], x, y, tileSize, xOffset, zOffset);
      }
    }
  }

  // Places one eligible wall prop when the cell is not adjacent to an existing prop.
  void TryPlaceProp(TileMetadata tile, int x, int y, float tileSize, float xOffset, float zOffset)
  {
    Vector2Int cell = new Vector2Int(x, y);
    if (IsNearOccupiedCell(cell))
    {
      return;
    }

    GameObject prop = TryGetWallProp(tile, out Quaternion rotation);
    if (prop == null)
    {
      return;
    }

    Vector3 position = new Vector3((x * tileSize) - xOffset, 0f, (y * tileSize) - zOffset);
    GameObject instance = Instantiate(prop, position, rotation, generatedRoot);
    instance.name = $"Prop [{x},{y}]";
    occupiedCells.Add(cell);
  }

  // Applies wall eligibility and chance rules before selecting a seeded prop variant and rotation.
  GameObject TryGetWallProp(TileMetadata tile, out Quaternion rotation)
  {
    rotation = Quaternion.identity;

    if (tile.Type != TileType.Wall || propSet.wallProps == null || propSet.wallProps.Length == 0)
    {
      return null;
    }

    if ((tile.Floors & CardinalDirections) == Direction.None)
    {
      return null;
    }

    if (propRandom.NextDouble() > propSet.wallPropChance)
    {
      return null;
    }

    rotation = DirectionUtility.GetFacingRotation(tile.Floors);
    return propSet.wallProps[propRandom.Next(propSet.wallProps.Length)];
  }

  // Tests the surrounding 3x3 cell neighbourhood for an existing prop.
  bool IsNearOccupiedCell(Vector2Int cell)
  {
    for (int dx = -1; dx <= 1; dx++)
    {
      for (int dy = -1; dy <= 1; dy++)
      {
        if (occupiedCells.Contains(new Vector2Int(cell.x + dx, cell.y + dy)))
        {
          return true;
        }
      }
    }

    return false;
  }

  // Finds or creates the parent transform for generated props.
  void EnsureGeneratedRoot()
  {
    if (generatedRoot != null)
    {
      return;
    }

    Transform existingRoot = transform.Find("Generated Props 3D");
    if (existingRoot != null)
    {
      generatedRoot = existingRoot;
      return;
    }

    GameObject root = new GameObject("Generated Props 3D");
    root.transform.SetParent(transform, false);
    generatedRoot = root.transform;
  }

  [ContextMenu("Clear")]
  // Removes generated props from the scene using editor-safe or runtime destruction.
  public void Clear()
  {
    if (generatedRoot == null)
    {
      Transform existingRoot = transform.Find("Generated Props 3D");
      if (existingRoot != null)
      {
        generatedRoot = existingRoot;
      }
    }

    if (generatedRoot == null)
    {
      return;
    }

    for (int i = generatedRoot.childCount - 1; i >= 0; i--)
    {
      GameObject child = generatedRoot.GetChild(i).gameObject;

#if UNITY_EDITOR
      if (!Application.isPlaying)
      {
        DestroyImmediate(child);
        continue;
      }
#endif

      Destroy(child);
    }
  }
}
