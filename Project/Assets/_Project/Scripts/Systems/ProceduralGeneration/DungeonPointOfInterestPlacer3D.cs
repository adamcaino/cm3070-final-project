using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Final placement pass. Reads the room roles BSPDungeonGenerator tagged (spawn/boss/loot) and drops the
/// corresponding gameplay object into each tagged room: the spawn room gets a portal against a wall (the
/// player's return point once the boss is dead) with the player placed at its authored spawn point; the
/// boss room gets a single portal at its center facing the corridor that leads into it - that portal is
/// what marks the room as a boss room at all, actual boss spawning/behaviour comes later and isn't
/// handled here; each loot room gets a single chest at its center, randomly either a normal or cursed
/// variant (the lock/enemy-wave behaviour a cursed chest implies isn't handled here either, this only
/// decides placement). Any prefab left unassigned falls back to a colored placeholder primitive.
/// </summary>
public class DungeonPointOfInterestPlacer3D : MonoBehaviour
{
  const Direction CardinalDirections = Direction.North | Direction.East | Direction.South | Direction.West;
  const string PlayerSpawnPointName = "PlayerSpawnPos";

  [SerializeField] BSPDungeonGenerator sourceGenerator;
  [SerializeField] DungeonTilePlacer3D tilePlacer;
  [SerializeField] PointOfInterestSet poiSet;
  [SerializeField] Transform player;
  [SerializeField] PlayerCameraOrbit playerCameraOrbit;

  Transform generatedRoot;
  System.Random poiRandom;

  [ContextMenu("Generate")]
  public void Generate()
  {
    if (sourceGenerator == null || tilePlacer == null || poiSet == null)
    {
      Debug.LogWarning($"{nameof(DungeonPointOfInterestPlacer3D)} is missing a source generator, tile placer, or point of interest set.");
      return;
    }

    IReadOnlyList<DungeonRoomInfo> rooms = sourceGenerator.LastRooms;
    TileMetadata[,] metadata = sourceGenerator.LastMetadata;
    if (rooms == null || metadata == null)
    {
      Debug.LogWarning($"{nameof(DungeonPointOfInterestPlacer3D)} found no generated rooms - generate the 2D map first.");
      return;
    }

    Clear();
    EnsureGeneratedRoot();

    poiRandom = new System.Random(sourceGenerator.LastUsedSeed);

    int gridWidth = metadata.GetLength(0);
    int gridHeight = metadata.GetLength(1);

    foreach (DungeonRoomInfo room in rooms)
    {
      switch (room.Role)
      {
        case RoomRole.Spawn:
          PlaceSpawn(room, metadata, gridWidth, gridHeight);
          break;
        case RoomRole.Boss:
          PlaceBoss(room, metadata, gridWidth, gridHeight);
          break;
        case RoomRole.Loot:
          PlaceLoot(room, gridWidth, gridHeight);
          break;
      }
    }
  }

  // Placed against a wall (rather than the room center) so a large portal prefab doesn't loom over
  // the middle of the room, and rotated to match that wall's facing so it reads as built into it.
  void PlaceSpawn(DungeonRoomInfo room, TileMetadata[,] metadata, int gridWidth, int gridHeight)
  {
    Vector2Int cell = room.Center;
    Quaternion rotation = Quaternion.identity;

    if (TryFindWallAgainstRoom(room.Bounds, metadata, gridWidth, gridHeight, out Vector2Int wallCell, out Direction facing))
    {
      cell = wallCell;
      rotation = DirectionUtility.GetFacingRotation(facing);
    }

    Vector3 worldPosition = tilePlacer.GridToWorld(cell, gridWidth, gridHeight);
    GameObject portal = SpawnPortal(worldPosition, rotation);
    PlacePlayer(portal, worldPosition);
  }

  bool TryFindWallAgainstRoom(RectInt bounds, TileMetadata[,] metadata, int gridWidth, int gridHeight, out Vector2Int wallCell, out Direction facing)
  {
    List<Vector2Int> candidates = GetRoomPerimeterCells(bounds);
    ShuffleInPlace(candidates);

    foreach (Vector2Int cell in candidates)
    {
      if (cell.x < 0 || cell.y < 0 || cell.x >= gridWidth || cell.y >= gridHeight)
      {
        continue;
      }

      TileMetadata tile = metadata[cell.x, cell.y];
      Direction cardinalFloors = tile.Floors & CardinalDirections;

      if (tile.Type != TileType.Wall || cardinalFloors == Direction.None)
      {
        continue;
      }

      wallCell = cell;
      facing = DirectionUtility.GetFirstCardinal(cardinalFloors);
      return true;
    }

    wallCell = default;
    facing = Direction.None;
    return false;
  }

  // The boss portal sits at the room's center rather than against a wall, so it needs the direction
  // from the center toward the corridor instead of a wall's own facing. A door tile's DoorRoomSide
  // points from the door back into the room, so the opposite of that points from the room out to the
  // door - which is the direction the portal should face to look like it opens onto that corridor.
  bool TryFindDoorFacing(RectInt bounds, TileMetadata[,] metadata, int gridWidth, int gridHeight, out Direction facing)
  {
    List<Vector2Int> candidates = GetRoomPerimeterCells(bounds);
    ShuffleInPlace(candidates);

    foreach (Vector2Int cell in candidates)
    {
      if (cell.x < 0 || cell.y < 0 || cell.x >= gridWidth || cell.y >= gridHeight)
      {
        continue;
      }

      TileMetadata tile = metadata[cell.x, cell.y];
      if (tile.Type != TileType.Door)
      {
        continue;
      }

      facing = DirectionUtility.GetOpposite(tile.DoorRoomSide);
      return true;
    }

    facing = Direction.None;
    return false;
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

  void ShuffleInPlace(List<Vector2Int> cells)
  {
    for (int i = cells.Count - 1; i > 0; i--)
    {
      int swapIndex = poiRandom.Next(i + 1);
      (cells[i], cells[swapIndex]) = (cells[swapIndex], cells[i]);
    }
  }

  GameObject SpawnPortal(Vector3 worldPosition, Quaternion rotation)
  {
    GameObject instance = poiSet.spawnPortalPrefab != null
      ? Instantiate(poiSet.spawnPortalPrefab, worldPosition, rotation)
      : CreatePlaceholder(poiSet.spawnPortalColor, worldPosition);

    instance.name = "Spawn Portal";
    instance.transform.SetParent(generatedRoot, false);
    return instance;
  }

  // Always at the room's center, facing whichever corridor connects to it - that's what makes this
  // the boss room. Falls back to facing north if no bordering door is found (shouldn't normally happen,
  // every non-spawn room is reachable through at least one door).
  void PlaceBoss(DungeonRoomInfo room, TileMetadata[,] metadata, int gridWidth, int gridHeight)
  {
    Quaternion rotation = Quaternion.identity;

    if (TryFindDoorFacing(room.Bounds, metadata, gridWidth, gridHeight, out Direction facing))
    {
      rotation = DirectionUtility.GetFacingRotation(facing);
    }

    Vector3 worldPosition = tilePlacer.GridToWorld(room.Center, gridWidth, gridHeight);
    GameObject instance = poiSet.bossPortalPrefab != null
      ? Instantiate(poiSet.bossPortalPrefab, worldPosition, rotation)
      : CreatePlaceholder(poiSet.bossPortalColor, worldPosition);

    instance.name = "Boss Portal";
    instance.transform.SetParent(generatedRoot, false);
  }

  // Always at the room's center. Which chest variant spawns is decided here (seeded, so still
  // reproducible per dungeon seed); the lock/enemy-wave behaviour a cursed chest implies is future work.
  void PlaceLoot(DungeonRoomInfo room, int gridWidth, int gridHeight)
  {
    bool isCursed = poiRandom.NextDouble() < poiSet.cursedChestChance;
    GameObject[] variants = isCursed ? poiSet.cursedChestPrefabs : poiSet.normalChestPrefabs;
    Color placeholderColor = isCursed ? poiSet.cursedChestColor : poiSet.normalChestColor;
    string label = isCursed ? "Loot Chest (Cursed)" : "Loot Chest (Normal)";

    Vector3 worldPosition = tilePlacer.GridToWorld(room.Center, gridWidth, gridHeight);
    SpawnFromSet(variants, placeholderColor, worldPosition, Quaternion.identity, label);
  }

  // The player needs to land at the portal's own PlayerSpawnPos child (its authored "step out here"
  // point) rather than the portal's pivot, so this only runs once the portal instance actually exists.
  void PlacePlayer(GameObject portalInstance, Vector3 fallbackWorldPosition)
  {
    if (player == null)
    {
      Debug.LogWarning($"{nameof(DungeonPointOfInterestPlacer3D)} has no player assigned - skipping spawn placement.");
      return;
    }

    Transform spawnPoint = portalInstance != null ? FindDeepChild(portalInstance.transform, PlayerSpawnPointName) : null;
    if (spawnPoint != null)
    {
      player.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
    }
    else
    {
      player.position = fallbackWorldPosition;
    }

    // Must run after the player's spawn rotation is set, and before PlayerLocomotion's first Update
    // slaves the player's facing back to the camera - otherwise the camera's leftover default yaw
    // would win instead of adopting the authored spawn facing.
    playerCameraOrbit?.SnapYawToTarget(player);
  }

  // Transform.Find only checks direct children, but PlayerSpawnPos sits a level deeper (under a
  // "Pivot" child), so it needs a recursive search rather than a plain Find.
  static Transform FindDeepChild(Transform root, string name)
  {
    foreach (Transform child in root)
    {
      if (child.name == name)
      {
        return child;
      }

      Transform match = FindDeepChild(child, name);
      if (match != null)
      {
        return match;
      }
    }

    return null;
  }

  void SpawnFromSet(GameObject[] variants, Color placeholderColor, Vector3 worldPosition, Quaternion rotation, string label)
  {
    GameObject prefab = variants == null || variants.Length == 0 ? null : variants[poiRandom.Next(variants.Length)];
    GameObject instance = prefab != null
      ? Instantiate(prefab, worldPosition, rotation)
      : CreatePlaceholder(placeholderColor, worldPosition);

    instance.name = label;
    instance.transform.SetParent(generatedRoot, false);
  }

  GameObject CreatePlaceholder(Color color, Vector3 position)
  {
    GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
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

    Transform existingRoot = transform.Find("Generated Points of Interest");
    if (existingRoot != null)
    {
      generatedRoot = existingRoot;
      return;
    }

    GameObject root = new GameObject("Generated Points of Interest");
    root.transform.SetParent(transform, false);
    generatedRoot = root.transform;
  }

  [ContextMenu("Clear")]
  public void Clear()
  {
    if (generatedRoot == null)
    {
      Transform existingRoot = transform.Find("Generated Points of Interest");
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
