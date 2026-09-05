using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Enemy placement pass. Walks every RoomRole.Normal room (Spawn/Boss/Loot rooms are POI territory, not
/// combat territory - see DungeonPointOfInterestPlacer3D), rolls one enemy type from EnemySet for the
/// whole room, and scatters however many copies the room's floor area calls for across its interior,
/// spacing them a cell apart so they don't stack on the same tile.
/// </summary>
public class DungeonEnemyPlacer3D : MonoBehaviour
{
  const float NavMeshSampleDistance = 2f;

  [SerializeField] BSPDungeonGenerator sourceGenerator;
  [SerializeField] DungeonTilePlacer3D tilePlacer;
  [SerializeField] EnemySet enemySet;

  Transform generatedRoot;
  System.Random enemyRandom;
  readonly HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();

  [ContextMenu("Generate")]
  public void Generate()
  {
    if (sourceGenerator == null || tilePlacer == null || enemySet == null)
    {
      Debug.LogWarning($"{nameof(DungeonEnemyPlacer3D)} is missing a source generator, tile placer, or enemy set.");
      return;
    }

    IReadOnlyList<DungeonRoomInfo> rooms = sourceGenerator.LastRooms;
    TileMetadata[,] metadata = sourceGenerator.LastMetadata;
    if (rooms == null || metadata == null)
    {
      Debug.LogWarning($"{nameof(DungeonEnemyPlacer3D)} found no generated rooms - generate the 2D map first.");
      return;
    }

    Clear();
    EnsureGeneratedRoot();

    enemyRandom = new System.Random(sourceGenerator.LastUsedSeed);
    occupiedCells.Clear();

    int gridWidth = metadata.GetLength(0);
    int gridHeight = metadata.GetLength(1);

    foreach (DungeonRoomInfo room in rooms)
    {
      if (room.Role == RoomRole.Normal)
      {
        PlaceEnemies(room, gridWidth, gridHeight);
      }
    }
  }

  void PlaceEnemies(DungeonRoomInfo room, int gridWidth, int gridHeight)
  {
    GameObject prefab = enemySet.enemyPrefabs == null || enemySet.enemyPrefabs.Length == 0
      ? null
      : enemySet.enemyPrefabs[enemyRandom.Next(enemySet.enemyPrefabs.Length)];

    int area = room.Bounds.width * room.Bounds.height;
    int count = Mathf.Clamp(Mathf.RoundToInt(area / enemySet.tilesPerEnemy), enemySet.minEnemiesPerRoom, enemySet.maxEnemiesPerRoom);

    for (int i = 0; i < count; i++)
    {
      Vector2Int cell = PickSpawnCell(room.Bounds);
      Vector3 worldPosition = tilePlacer.GridToWorld(cell, gridWidth, gridHeight);

      if (!NavMesh.SamplePosition(worldPosition, out NavMeshHit navMeshHit, NavMeshSampleDistance, NavMesh.AllAreas))
      {
        string prefabName = prefab != null ? prefab.name : "Placeholder";
        Debug.LogWarning($"{nameof(DungeonEnemyPlacer3D)} skipped {prefabName} in room {room.RoomId} at cell {cell} (world position {worldPosition}) because no NavMesh was found within {NavMeshSampleDistance} units.");
        continue;
      }

      GameObject instance = prefab != null
        ? Instantiate(prefab, navMeshHit.position, Quaternion.identity)
        : CreatePlaceholder(enemySet.placeholderColor, worldPosition);

      instance.name = $"Enemy [{cell.x},{cell.y}]";
      instance.transform.SetParent(generatedRoot, false);
    }
  }

  // Tries a handful of times to find a cell not touching an already-placed enemy so they don't stack
  // shoulder-to-shoulder; falls back to whatever cell it last rolled rather than skipping the spawn -
  // a slightly-too-close enemy beats one that's silently missing.
  Vector2Int PickSpawnCell(RectInt bounds)
  {
    Vector2Int cell = RandomCellIn(bounds);

    for (int attempt = 0; attempt < 20 && IsNearOccupiedCell(cell); attempt++)
    {
      cell = RandomCellIn(bounds);
    }

    occupiedCells.Add(cell);
    return cell;
  }

  Vector2Int RandomCellIn(RectInt bounds)
  {
    return new Vector2Int(enemyRandom.Next(bounds.xMin, bounds.xMax), enemyRandom.Next(bounds.yMin, bounds.yMax));
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

  GameObject CreatePlaceholder(Color color, Vector3 position)
  {
    GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Capsule);
    placeholder.transform.position = position + (Vector3.up * 0.5f);
    Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
    placeholder.GetComponent<Renderer>().sharedMaterial = new Material(shader) { color = color };
    return placeholder;
  }

  void EnsureGeneratedRoot()
  {
    if (generatedRoot != null)
    {
      return;
    }

    Transform existingRoot = transform.Find("Generated Enemies");
    if (existingRoot != null)
    {
      generatedRoot = existingRoot;
      return;
    }

    GameObject root = new GameObject("Generated Enemies");
    root.transform.SetParent(transform, false);
    generatedRoot = root.transform;
  }

  [ContextMenu("Clear")]
  public void Clear()
  {
    if (generatedRoot == null)
    {
      Transform existingRoot = transform.Find("Generated Enemies");
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
