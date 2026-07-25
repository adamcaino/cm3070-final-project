using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Placeholder 3D placement pass. Reads TileMetadata from a DungeonGridGenerator2D and instantiates a
/// prefab (or a colored primitive stand-in, if the DungeonTileSet's prefab field is left empty) per
/// tile, positioned and rotated to match.
///
/// Conventions assumed here (revisit once real prefabs exist and can dictate their own pivot/facing):
/// - Grid North (+Y in the 2D grid) maps to world +Z, East (+X) maps to world +X.
/// - A prefab's forward (+Z) is treated as its decorated/front face.
/// - Wall tiles rotate to face their open (Floors) direction.
/// - Corner tiles (explicitly flagged by the generator at room corners) rotate based on which diagonal
///   direction has floor (see GetCornerRotation).
/// - Door tiles rotate to face their DoorRoomSide (the wide/room side, as opposed to the 1-tile-wide
///   corridor side) so an asymmetric door prefab is oriented consistently on all four sides.
/// </summary>
public class DungeonTilePlacer3D : MonoBehaviour
{
  [SerializeField] DungeonGridGenerator2D sourceGenerator;
  [SerializeField] DungeonTileSet tileSet;
  [SerializeField, Min(0.1f)] float tileSize = 2f;
  [SerializeField] Transform generatedRoot;

  readonly Dictionary<TileType, Material> placeholderMaterials = new Dictionary<TileType, Material>();

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
    Vector3 offsetPosition = position + (rotation * GetPivotOffset(type));
    return prefab != null ? Instantiate(prefab, offsetPosition, rotation) : CreatePlaceholder(type, offsetPosition, rotation);
  }

  Vector3 GetPivotOffset(TileType type)
  {
    switch (type)
    {
      case TileType.Floor:
        return tileSet.floorPivotOffset;
      case TileType.Wall:
        return tileSet.wallPivotOffset;
      case TileType.Door:
        return tileSet.doorPivotOffset;
      case TileType.Corner:
        return tileSet.cornerPivotOffset;
      default:
        return Vector3.zero;
    }
  }

  GameObject GetPrefab(TileType type)
  {
    switch (type)
    {
      case TileType.Floor:
        return tileSet.floorPrefab;
      case TileType.Wall:
        return tileSet.wallPrefab;
      case TileType.Door:
        return tileSet.doorPrefab;
      case TileType.Corner:
        return tileSet.cornerPrefab;
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
      case TileType.Corner:
        return new Vector3(tileSize, tileSize, tileSize);
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
      case TileType.Corner:
        return tileSet.cornerColor;
      default:
        return Color.magenta;
    }
  }

  Quaternion GetRotation(TileMetadata tile)
  {
    switch (tile.Type)
    {
      case TileType.Wall:
        return GetFacingRotation(tile.Floors);
      case TileType.Door:
        return GetFacingRotation(tile.DoorRoomSide);
      case TileType.Corner:
        return GetCornerRotation(tile.Floors);
      default:
        return Quaternion.identity;
    }
  }

  Quaternion GetFacingRotation(Direction floors)
  {
    Vector3 facing = GetFacingDirection(floors);
    return facing == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(facing, Vector3.up);
  }

  Quaternion GetCornerRotation(Direction floors)
  {
    if ((floors & Direction.NorthEast) != Direction.None)
    {
      return Quaternion.identity;
    }

    if ((floors & Direction.SouthEast) != Direction.None)
    {
      return Quaternion.Euler(0f, 90f, 0f);
    }

    if ((floors & Direction.SouthWest) != Direction.None)
    {
      return Quaternion.Euler(0f, 180f, 0f);
    }

    if ((floors & Direction.NorthWest) != Direction.None)
    {
      return Quaternion.Euler(0f, 270f, 0f);
    }

    return Quaternion.identity;
  }

  Vector3 GetFacingDirection(Direction floors)
  {
    if ((floors & Direction.North) != Direction.None)
    {
      return Vector3.forward;
    }

    if ((floors & Direction.East) != Direction.None)
    {
      return Vector3.right;
    }

    if ((floors & Direction.South) != Direction.None)
    {
      return Vector3.back;
    }

    if ((floors & Direction.West) != Direction.None)
    {
      return Vector3.left;
    }

    return Vector3.zero;
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
