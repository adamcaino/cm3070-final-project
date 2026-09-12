using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Places seeded regular and tough enemy groups in normal rooms according to room area and progression.
// Spawn, boss, and loot rooms are handled by their dedicated placement passes.
public class DungeonEnemyPlacer3D : MonoBehaviour
{
  const float NavMeshSampleDistance = 2f;
  const int MaxToughEnemiesPerRoomCap = 2;

  [SerializeField] BSPDungeonGenerator sourceGenerator;
  [SerializeField] DungeonTilePlacer3D tilePlacer;
  [SerializeField] EnemySet enemySet;

  Transform generatedRoot;
  System.Random enemyRandom;
  readonly HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();

  public int RegularEnemyCount { get; private set; }
  public int ToughEnemyCount { get; private set; }

  [ContextMenu("Generate")]
  // Reads generated rooms, resets prior enemies, and places enemies in normal rooms.
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
    RegularEnemyCount = 0;
    ToughEnemyCount = 0;

    int gridWidth = metadata.GetLength(0);
    int gridHeight = metadata.GetLength(1);

    foreach (DungeonRoomInfo room in rooms)
    {
      if (room.Role == RoomRole.Normal)
      {
        PlaceEnemies(room, gridWidth, gridHeight);
      }
    }

    Debug.Log($"{nameof(DungeonEnemyPlacer3D)} placed {RegularEnemyCount} regular and {ToughEnemyCount} tough enemies.");
  }

  // Calculates room density and difficulty, then spawns regular and tough enemy groups.
  void PlaceEnemies(DungeonRoomInfo room, int gridWidth, int gridHeight)
  {
    float difficulty = GetRoomDifficulty(room);
    int area = room.Bounds.width * room.Bounds.height;
    int targetByArea = Mathf.RoundToInt(area / enemySet.tilesPerEnemy);
    int minCount = Mathf.RoundToInt(Mathf.Lerp(enemySet.minEnemiesNearSpawn, enemySet.minEnemiesNearBoss, difficulty));
    int maxCount = Mathf.RoundToInt(Mathf.Lerp(enemySet.maxEnemiesNearSpawn, enemySet.maxEnemiesNearBoss, difficulty));
    int roomMin = Mathf.Max(1, Mathf.Min(minCount, maxCount));
    int roomMax = Mathf.Max(roomMin, maxCount);
    int totalCount = Mathf.Clamp(targetByArea, roomMin, roomMax);

    int toughCount = CalculateToughEnemyCount(difficulty, totalCount);
    int regularCount = Mathf.Max(0, totalCount - toughCount);

    GameObject regularPrefab = enemySet.enemyPrefabs == null || enemySet.enemyPrefabs.Length == 0
      ? null
      : enemySet.enemyPrefabs[enemyRandom.Next(enemySet.enemyPrefabs.Length)];

    GameObject toughPrefab = enemySet.toughEnemyPrefabs == null || enemySet.toughEnemyPrefabs.Length == 0
      ? null
      : enemySet.toughEnemyPrefabs[enemyRandom.Next(enemySet.toughEnemyPrefabs.Length)];

    SpawnEnemiesOfType(regularPrefab, regularCount, room, gridWidth, gridHeight, "Enemy");
    SpawnEnemiesOfType(toughPrefab, toughCount, room, gridWidth, gridHeight, "Tough Enemy");
  }

  // Converts the room's distance from spawn into normalized progression difficulty.
  float GetRoomDifficulty(DungeonRoomInfo room)
  {
    if (room.SpawnToBossDistance <= 0 || room.DistanceFromSpawn <= 0)
    {
      return 0f;
    }

    return Mathf.Clamp01((float)room.DistanceFromSpawn / room.SpawnToBossDistance);
  }

  // Calculates the capped tough-enemy count after its progression threshold is reached.
  int CalculateToughEnemyCount(float difficulty, int totalCount)
  {
    if (totalCount <= 0 || enemySet.toughEnemyPrefabs == null || enemySet.toughEnemyPrefabs.Length == 0)
    {
      return 0;
    }

    int hardCap = Mathf.Min(MaxToughEnemiesPerRoomCap, enemySet.maxToughEnemiesPerRoom);
    int countCap = Mathf.Min(hardCap, totalCount);
    if (countCap <= 0)
    {
      return 0;
    }

    if (difficulty < enemySet.toughEnemyStartDistance)
    {
      return 0;
    }

    float toughRamp = Mathf.InverseLerp(enemySet.toughEnemyStartDistance, 1f, difficulty);
    return Mathf.Clamp(Mathf.RoundToInt(toughRamp * countCap), 0, countCap);
  }

  // Samples NavMesh positions and instantiates one group of the requested enemy type.
  void SpawnEnemiesOfType(GameObject prefab, int count, DungeonRoomInfo room, int gridWidth, int gridHeight, string label)
  {
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

      instance.name = $"{label} [{cell.x},{cell.y}]";
      instance.transform.SetParent(generatedRoot, false);

      if (label == "Tough Enemy")
      {
        ToughEnemyCount++;
      }
      else
      {
        RegularEnemyCount++;
      }
    }
  }

  // Selects a room cell with local spacing from earlier enemy placements.
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

  // Returns a seeded random cell inside the room bounds.
  Vector2Int RandomCellIn(RectInt bounds)
  {
    return new Vector2Int(enemyRandom.Next(bounds.xMin, bounds.xMax), enemyRandom.Next(bounds.yMin, bounds.yMax));
  }

  // Tests the surrounding 3x3 neighbourhood for an occupied enemy cell.
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

  // Creates a coloured capsule when no enemy prefab is configured.
  GameObject CreatePlaceholder(Color color, Vector3 position)
  {
    GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Capsule);
    placeholder.transform.position = position + (Vector3.up * 0.5f);
    Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
    placeholder.GetComponent<Renderer>().sharedMaterial = new Material(shader) { color = color };
    return placeholder;
  }

  // Finds or creates the parent transform for generated enemies.
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
  // Removes generated enemies from the scene.
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
