using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DungeonBossPlacer3D : MonoBehaviour
{
  const float TriggerZoneHeight = 4f;
  const float NavMeshSampleDistance = 2f;

  [SerializeField] BSPDungeonGenerator sourceGenerator;
  [SerializeField] DungeonTilePlacer3D tilePlacer;
  [SerializeField] BossSet bossSet;

  Transform generatedRoot;

  [ContextMenu("Generate")]
  public void Generate()
  {
    if (sourceGenerator == null || tilePlacer == null || bossSet == null)
    {
      Debug.LogWarning($"{nameof(DungeonBossPlacer3D)} is missing a source generator, tile placer, or boss set.");
      return;
    }

    IReadOnlyList<DungeonRoomInfo> rooms = sourceGenerator.LastRooms;
    TileMetadata[,] metadata = sourceGenerator.LastMetadata;
    if (rooms == null || metadata == null)
    {
      Debug.LogWarning($"{nameof(DungeonBossPlacer3D)} found no generated rooms - generate the 2D map first.");
      return;
    }

    Clear();
    EnsureGeneratedRoot();

    int gridWidth = metadata.GetLength(0);
    int gridHeight = metadata.GetLength(1);

    foreach (DungeonRoomInfo room in rooms)
    {
      if (room.Role == RoomRole.Boss)
      {
        PlaceBoss(room, metadata, gridWidth, gridHeight);
      }
    }
  }

  void PlaceBoss(DungeonRoomInfo room, TileMetadata[,] metadata, int gridWidth, int gridHeight)
  {
    List<Door> doors = FindRoomDoors(room.Bounds, metadata, gridWidth, gridHeight, room.RoomId);
    Vector3 worldPosition = tilePlacer.GridToWorld(room.Center, gridWidth, gridHeight);

    if (!NavMesh.SamplePosition(worldPosition, out NavMeshHit navMeshHit, NavMeshSampleDistance, NavMesh.AllAreas))
    {
      Debug.LogWarning($"{nameof(DungeonBossPlacer3D)} skipped boss in room {room.RoomId} at world position {worldPosition} because no NavMesh was found within {NavMeshSampleDistance} units.");
      return;
    }

    worldPosition = navMeshHit.position;

    // Select a random boss prefab from the array if available.
    GameObject bossPrefab = bossSet.bossPrefabs != null && bossSet.bossPrefabs.Length > 0
      ? bossSet.bossPrefabs[Random.Range(0, bossSet.bossPrefabs.Length)]
      : null;

    // Instantiate the selected boss prefab, or create a placeholder if none is available.
    GameObject instance = bossPrefab != null
      ? Instantiate(bossPrefab, worldPosition, Quaternion.identity)
      : CreatePlaceholder(bossSet.placeholderColor, worldPosition);

    instance.name = "Boss";
    instance.transform.SetParent(generatedRoot, false);

    if (!instance.TryGetComponent(out BossRoomEncounter encounter) || !instance.TryGetComponent(out Health health))
    {
      Debug.LogWarning($"{nameof(DungeonBossPlacer3D)}: Boss prefab is missing a BossRoomEncounter or Health component - room lock, boss UI, and music won't function for this boss.", instance);
      return;
    }

    encounter.Configure(health, doors);
    encounter.SetMusic(bossSet.bossMusic, bossSet.clearAmbienceMusic);

    GameObject triggerZone = CreateEncounterTriggerZone(room.Bounds, worldPosition, encounter);
    triggerZone.transform.SetParent(generatedRoot, false);
  }

  // Every Door cell on the room's perimeter, resolved to its placed instance via the tile placer and
  // tagged with which room it borders - the same lookup a future loot-room placer would reuse.
  List<Door> FindRoomDoors(RectInt bounds, TileMetadata[,] metadata, int gridWidth, int gridHeight, int roomId)
  {
    List<Door> doors = new List<Door>();

    foreach (Vector2Int cell in GetRoomPerimeterCells(bounds))
    {
      if (cell.x < 0 || cell.y < 0 || cell.x >= gridWidth || cell.y >= gridHeight)
      {
        continue;
      }

      if (metadata[cell.x, cell.y].Type != TileType.Door)
      {
        continue;
      }

      if (!tilePlacer.TryGetDoorInstance(cell, out GameObject instance) || !instance.TryGetComponent(out Door door))
      {
        continue;
      }

      if (!instance.TryGetComponent(out RoomEntranceDoor entrance))
      {
        entrance = instance.AddComponent<RoomEntranceDoor>();
      }
      entrance.Configure(RoomRole.Boss, roomId);

      doors.Add(door);
    }

    return doors;
  }

  static List<Vector2Int> GetRoomPerimeterCells(RectInt bounds)
  {
    List<Vector2Int> cells = new List<Vector2Int>();

    for (int x = bounds.xMin; x < bounds.xMax; x++)
    {
      cells.Add(new Vector2Int(x, bounds.yMin - 1));
      cells.Add(new Vector2Int(x, bounds.yMax));
    }

    for (int y = bounds.yMin; y < bounds.yMax; y++)
    {
      cells.Add(new Vector2Int(bounds.xMin - 1, y));
      cells.Add(new Vector2Int(bounds.xMax, y));
    }

    return cells;
  }

  GameObject CreateEncounterTriggerZone(RectInt bounds, Vector3 roomCenterWorld, RoomEncounter encounter)
  {
    GameObject zone = new GameObject("Boss Encounter Trigger");
    zone.transform.position = roomCenterWorld + (Vector3.up * (TriggerZoneHeight * 0.5f));

    BoxCollider trigger = zone.AddComponent<BoxCollider>();
    trigger.isTrigger = true;
    trigger.size = new Vector3(bounds.width * tilePlacer.TileSize, TriggerZoneHeight, bounds.height * tilePlacer.TileSize);

    TriggerRelay relay = zone.AddComponent<TriggerRelay>();
    relay.Configure(encounter.gameObject);

    return zone;
  }

  GameObject CreatePlaceholder(Color color, Vector3 position)
  {
    GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Capsule);
    placeholder.transform.position = position + (Vector3.up * 1f);
    placeholder.transform.localScale = Vector3.one * 2f;
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

    Transform existingRoot = transform.Find("Generated Boss");
    if (existingRoot != null)
    {
      generatedRoot = existingRoot;
      return;
    }

    GameObject root = new GameObject("Generated Boss");
    root.transform.SetParent(transform, false);
    generatedRoot = root.transform;
  }

  [ContextMenu("Clear")]
  public void Clear()
  {
    if (generatedRoot == null)
    {
      Transform existingRoot = transform.Find("Generated Boss");
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
