using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sparse decoration pass. Run after DungeonTilePlacer3D has generated the tile layout: walks the same
/// TileMetadata grid and, for eligible tiles, rolls a seeded chance to spawn a prop.
/// - Wall props require a straight wall tile (a cardinal Floors direction) and face the same way the
///   wall itself does.
/// - Floor props require a Floor tile against a wall, with open floor to at least one side of that
///   wall (so it's inside a room rather than a 1-tile-wide corridor), and face away from the wall into
///   the room.
/// A cell adjacent to an already-placed prop is skipped, so props don't cluster shoulder-to-shoulder.
/// Placement is then jittered (still seeded/reproducible) so it doesn't look grid-snapped: wall props
/// slide left/right along their wall only, floor props get an X/Z offset and extra yaw rotation.
/// </summary>
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

  void TryPlaceProp(TileMetadata tile, int x, int y, float tileSize, float xOffset, float zOffset)
  {
    Vector2Int cell = new Vector2Int(x, y);
    if (IsNearOccupiedCell(cell))
    {
      return;
    }

    GameObject prop = GetRotationAndProp(tile, out Quaternion rotation);
    if (prop == null)
    {
      return;
    }

    rotation = ApplyRotationJitter(tile.Type, rotation);

    Vector3 basePosition = new Vector3((x * tileSize) - xOffset, 0f, (y * tileSize) - zOffset);
    Vector3 localJitter = GetPositionJitter(tile.Type, tileSize);
    Vector3 position = basePosition + (rotation * localJitter);

    GameObject instance = Instantiate(prop, position, rotation, generatedRoot);
    instance.name = $"Prop [{x},{y}]";
    occupiedCells.Add(cell);
  }

  // Local X is left/right along whichever way the prop is currently facing, local Z is
  // toward/away from that facing direction - so wall props only ever slide along their wall.
  Vector3 GetPositionJitter(TileType type, float tileSize)
  {
    switch (type)
    {
      case TileType.Wall:
        return new Vector3(RandomJitter(propSet.wallPositionJitter * tileSize), 0f, 0f);
      case TileType.Floor:
        float maxOffset = propSet.floorPositionJitter * tileSize;
        return new Vector3(RandomJitter(maxOffset), 0f, RandomJitter(maxOffset));
      default:
        return Vector3.zero;
    }
  }

  Quaternion ApplyRotationJitter(TileType type, Quaternion rotation)
  {
    if (type != TileType.Floor || propSet.floorRotationJitterDegrees <= 0f)
    {
      return rotation;
    }

    float yawJitter = RandomJitter(propSet.floorRotationJitterDegrees);
    return rotation * Quaternion.Euler(0f, yawJitter, 0f);
  }

  float RandomJitter(float maxAbsValue)
  {
    return (float)((propRandom.NextDouble() * 2.0) - 1.0) * maxAbsValue;
  }

  GameObject GetRotationAndProp(TileMetadata tile, out Quaternion rotation)
  {
    rotation = Quaternion.identity;

    switch (tile.Type)
    {
      case TileType.Wall:
        return TryGetWallProp(tile, out rotation);
      case TileType.Floor:
        return TryGetFloorProp(tile, out rotation);
      default:
        return null;
    }
  }

  GameObject TryGetWallProp(TileMetadata tile, out Quaternion rotation)
  {
    rotation = Quaternion.identity;

    if (propSet.wallProps == null || propSet.wallProps.Length == 0)
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

  GameObject TryGetFloorProp(TileMetadata tile, out Quaternion rotation)
  {
    rotation = Quaternion.identity;

    if (propSet.floorProps == null || propSet.floorProps.Length == 0)
    {
      return null;
    }

    Direction wallDirection = DirectionUtility.GetFirstCardinal(tile.Walls);
    if (wallDirection == Direction.None || !IsRoomFloor(tile, wallDirection))
    {
      return null;
    }

    if (propRandom.NextDouble() > propSet.floorPropChance)
    {
      return null;
    }

    rotation = DirectionUtility.GetFacingRotation(DirectionUtility.GetOpposite(wallDirection));
    return propSet.floorProps[propRandom.Next(propSet.floorProps.Length)];
  }

  bool IsRoomFloor(TileMetadata tile, Direction wallDirection)
  {
    (Direction perpendicularA, Direction perpendicularB) = DirectionUtility.GetPerpendicularCardinals(wallDirection);
    return (tile.Floors & (perpendicularA | perpendicularB)) != Direction.None;
  }

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
