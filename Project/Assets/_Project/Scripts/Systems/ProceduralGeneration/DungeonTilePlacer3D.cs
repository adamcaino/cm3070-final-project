using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Placeholder 3D placement pass. Reads TileMetadata from a DungeonGridGenerator2D and instantiates a
/// prefab variant (or a colored primitive stand-in, if the DungeonTileSet's variant array for that
/// type is empty) per tile, positioned and rotated to match. Variant choice is randomized with a
/// System.Random seeded from the source generator's LastUsedSeed, so it's reproducible per dungeon seed.
///
/// Conventions assumed here (revisit once real prefabs exist and can dictate their own pivot/facing):
/// - Grid North (+Y in the 2D grid) maps to world +Z, East (+X) maps to world +X.
/// - A prefab's forward (+Z) is treated as its decorated/front face.
/// - Wall tiles rotate to face their open (Floors) direction.
/// - Door tiles rotate to face their DoorRoomSide (the wide/room side, as opposed to the 1-tile-wide
///   corridor side) so an asymmetric door prefab is oriented consistently on all four sides.
/// </summary>
public class DungeonTilePlacer3D : MonoBehaviour
{
  [SerializeField] DungeonGridGenerator2D sourceGenerator;
  [SerializeField] DungeonTileSet tileSet;
  [SerializeField, Min(0.1f)] float tileSize = 2f;

  Transform generatedRoot;
  readonly Dictionary<TileType, Material> placeholderMaterials = new Dictionary<TileType, Material>();
  System.Random variantRandom;

  public float TileSize => tileSize;

  // Shared with other placement passes (props, points of interest) so every stage maps a grid cell to
  // the same world position this one used, without each duplicating the offset math.
  public Vector3 GridToWorld(Vector2Int gridPosition, int gridWidth, int gridHeight)
  {
    float xOffset = (gridWidth - 1) * tileSize * 0.5f;
    float zOffset = (gridHeight - 1) * tileSize * 0.5f;
    return new Vector3((gridPosition.x * tileSize) - xOffset, 0f, (gridPosition.y * tileSize) - zOffset);
  }

  [ContextMenu("Generate")]
  public void Generate()
  {
    if (sourceGenerator == null || tileSet == null)
    {
      Debug.LogWarning($"{nameof(DungeonTilePlacer3D)} is missing a source generator or tile set.");
      return;
    }

    TileMetadata[,] metadata = sourceGenerator.LastMetadata;
    if (metadata == null)
    {
      Debug.LogWarning($"{nameof(DungeonTilePlacer3D)} found no generated metadata - generate the 2D map first.");
      return;
    }

    Clear();
    EnsureGeneratedRoot();

    variantRandom = new System.Random(sourceGenerator.LastUsedSeed);

    int width = metadata.GetLength(0);
    int height = metadata.GetLength(1);
    float xOffset = (width - 1) * tileSize * 0.5f;
    float zOffset = (height - 1) * tileSize * 0.5f;

    for (int x = 0; x < width; x++)
    {
      for (int y = 0; y < height; y++)
      {
        PlaceTile(metadata[x, y], x, y, xOffset, zOffset);
      }
    }
  }

  void PlaceTile(TileMetadata tile, int x, int y, float xOffset, float zOffset)
  {
    if (tile.Type == TileType.Empty)
    {
      return;
    }

    Vector3 position = new Vector3((x * tileSize) - xOffset, 0f, (y * tileSize) - zOffset);
    Quaternion rotation = GetRotation(tile);

    GameObject instance = InstantiateTile(tile.Type, position, rotation);
    instance.name = $"{tile.Type} [{x},{y}]";
    instance.transform.SetParent(generatedRoot, false);
  }

  GameObject InstantiateTile(TileType type, Vector3 position, Quaternion rotation)
  {
    GameObject prefab = GetPrefab(type);
    return prefab != null ? Instantiate(prefab, position, rotation) : CreatePlaceholder(type, position, rotation);
  }

  GameObject GetPrefab(TileType type)
  {
    GameObject[] variants = GetPrefabVariants(type);
    return variants == null || variants.Length == 0 ? null : variants[variantRandom.Next(variants.Length)];
  }

  GameObject[] GetPrefabVariants(TileType type)
  {
    switch (type)
    {
      case TileType.Floor:
        return tileSet.floorPrefabs;
      case TileType.Wall:
        return tileSet.wallPrefabs;
      case TileType.Door:
        return tileSet.doorPrefabs;
      default:
        return null;
    }
  }

  GameObject CreatePlaceholder(TileType type, Vector3 position, Quaternion rotation)
  {
    GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
    placeholder.transform.SetPositionAndRotation(position, rotation);
    placeholder.transform.localScale = GetPlaceholderScale(type);
    placeholder.GetComponent<Renderer>().sharedMaterial = GetOrCreatePlaceholderMaterial(type);
    return placeholder;
  }

  Vector3 GetPlaceholderScale(TileType type)
  {
    switch (type)
    {
      case TileType.Floor:
        return new Vector3(tileSize, 0.1f, tileSize);
      case TileType.Wall:
        return new Vector3(tileSize, tileSize, 0.2f);
      case TileType.Door:
        return new Vector3(tileSize * 0.6f, tileSize * 0.8f, 0.2f);
      default:
        return Vector3.one * tileSize;
    }
  }

  Material GetOrCreatePlaceholderMaterial(TileType type)
  {
    if (placeholderMaterials.TryGetValue(type, out Material material) && material != null)
    {
      return material;
    }

    Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
    material = new Material(shader) { color = GetPlaceholderColor(type) };
    placeholderMaterials[type] = material;
    return material;
  }

  Color GetPlaceholderColor(TileType type)
  {
    switch (type)
    {
      case TileType.Floor:
        return tileSet.floorColor;
      case TileType.Wall:
        return tileSet.wallColor;
      case TileType.Door:
        return tileSet.doorColor;
      default:
        return Color.magenta;
    }
  }

  Quaternion GetRotation(TileMetadata tile)
  {
    switch (tile.Type)
    {
      case TileType.Wall:
        return DirectionUtility.GetFacingRotation(tile.Floors);
      case TileType.Door:
        return DirectionUtility.GetFacingRotation(tile.DoorRoomSide);
      default:
        return Quaternion.identity;
    }
  }

  void EnsureGeneratedRoot()
  {
    if (generatedRoot != null)
    {
      return;
    }

    Transform existingRoot = transform.Find("Generated Dungeon 3D");
    if (existingRoot != null)
    {
      generatedRoot = existingRoot;
      return;
    }

    GameObject root = new GameObject("Generated Dungeon 3D");
    root.transform.SetParent(transform, false);
    generatedRoot = root.transform;
  }

  [ContextMenu("Clear")]
  public void Clear()
  {
    if (generatedRoot == null)
    {
      Transform existingRoot = transform.Find("Generated Dungeon 3D");
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
