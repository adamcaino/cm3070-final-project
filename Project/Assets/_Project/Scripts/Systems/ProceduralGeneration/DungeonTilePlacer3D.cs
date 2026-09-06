using System.Collections.Generic;
using UnityEngine;

// Converts generated tile metadata into seeded 3D tile instances. Grid north maps to world +Z,
// wall tiles face their open side, and doors face the room side recorded in their metadata.
public class DungeonTilePlacer3D : MonoBehaviour
{
  [SerializeField] DungeonGridGenerator2D sourceGenerator;
  [SerializeField] DungeonTileSet tileSet;
  [SerializeField, Min(0.1f)] float tileSize = 2f;

  Transform generatedRoot;
  readonly Dictionary<TileType, Material> placeholderMaterials = new Dictionary<TileType, Material>();
  readonly Dictionary<Vector2Int, GameObject> placedDoors = new Dictionary<Vector2Int, GameObject>();
  System.Random variantRandom;

  public float TileSize => tileSize;

  // Returns the placed door associated with a generated grid cell.
  public bool TryGetDoorInstance(Vector2Int cell, out GameObject instance)
  {
    return placedDoors.TryGetValue(cell, out instance);
  }

  // Converts a grid coordinate into the centred world-space tile position.
  public Vector3 GridToWorld(Vector2Int gridPosition, int gridWidth, int gridHeight)
  {
    float xOffset = (gridWidth - 1) * tileSize * 0.5f;
    float zOffset = (gridHeight - 1) * tileSize * 0.5f;
    return new Vector3((gridPosition.x * tileSize) - xOffset, 0f, (gridPosition.y * tileSize) - zOffset);
  }

  [ContextMenu("Generate")]
  // Validates source data, resets prior output, and instantiates every non-empty tile.
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

  // Places one tile at its grid position and records door instances for later passes.
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

    if (tile.Type == TileType.Door)
    {
      placedDoors[new Vector2Int(x, y)] = instance;
    }
  }

  // Instantiates a selected prefab or creates a placeholder when no variant exists.
  GameObject InstantiateTile(TileType type, Vector3 position, Quaternion rotation)
  {
    GameObject prefab = GetPrefab(type);
    return prefab != null ? Instantiate(prefab, position, rotation) : CreatePlaceholder(type, position, rotation);
  }

  // Selects a seeded prefab variant for the requested tile type.
  GameObject GetPrefab(TileType type)
  {
    GameObject[] variants = GetPrefabVariants(type);
    return variants == null || variants.Length == 0 ? null : variants[variantRandom.Next(variants.Length)];
  }

  // Returns the configured variant array associated with a tile type.
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

  // Creates a primitive stand-in with the scale and colour for a tile type.
  GameObject CreatePlaceholder(TileType type, Vector3 position, Quaternion rotation)
  {
    GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
    placeholder.transform.SetPositionAndRotation(position, rotation);
    placeholder.transform.localScale = GetPlaceholderScale(type);
    placeholder.GetComponent<Renderer>().sharedMaterial = GetOrCreatePlaceholderMaterial(type);
    return placeholder;
  }

  // Returns the primitive dimensions used for an unassigned tile prefab.
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

  // Retrieves or creates the shared placeholder material for a tile type.
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

  // Maps a tile type to its configured placeholder colour.
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

  // Determines prefab rotation from the tile's exposed floor or room-side direction.
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

  // Finds or creates the parent transform for generated 3D tiles.
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
  // Removes generated tile objects and clears the placed-door lookup.
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

    // NavMesh baking runs synchronously right after regeneration in the same Start(); a deferred
    // Destroy() would leave last level's tiles queryable by NavMeshSurface.BuildNavMesh() until end
    // of frame, baking a navmesh over flooring that no longer visually exists.
    for (int i = generatedRoot.childCount - 1; i >= 0; i--)
    {
      DestroyImmediate(generatedRoot.GetChild(i).gameObject);
    }

    placedDoors.Clear();
  }
}
