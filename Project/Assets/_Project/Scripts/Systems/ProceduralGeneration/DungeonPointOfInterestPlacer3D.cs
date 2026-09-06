using System.Collections.Generic;
using UnityEngine;

// Places the spawn portal and loot points selected from generated room roles.
// The pass also detaches the player from the portal and refreshes runtime camera references.
public class DungeonPointOfInterestPlacer3D : MonoBehaviour
{
  const Direction CardinalDirections = Direction.North | Direction.East | Direction.South | Direction.West;

  [SerializeField] BSPDungeonGenerator sourceGenerator;
  [SerializeField] DungeonTilePlacer3D tilePlacer;
  [SerializeField] PointOfInterestSet poiSet;
  [SerializeField] Transform player;
  [SerializeField] PlayerCameraOrbit playerCameraOrbit;

  Transform generatedRoot;
  System.Random poiRandom;

  [ContextMenu("Generate")]
  // Resolves runtime references, reads room roles, and places each supported point of interest.
  public void Generate()
  {
    ResolveRuntimeReferences();

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
        case RoomRole.Loot:
          PlaceLoot(room, gridWidth, gridHeight);
          break;
      }
    }
  }

  // Finds the player and its camera orbit component when they were not assigned in the Inspector.
  void ResolveRuntimeReferences()
  {
    GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
    if (playerObject != null)
    {
      player = playerObject.transform;
    }
    else
    {
      PlayerDeathHandler deathHandler = FindFirstObjectByType<PlayerDeathHandler>();
      player = deathHandler != null ? deathHandler.transform : null;
    }

    if (player != null)
    {
      playerCameraOrbit = player.GetComponent<PlayerCameraOrbit>();
    }
  }

  // Places the spawn portal against a suitable room wall and relocates the player into it.
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
    PlacePlayer(portal);
  }

  // Finds a shuffled eligible wall cell and returns the direction its room floor faces.
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

  // Returns the cardinal cells immediately outside a room rectangle.
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

  // Shuffles perimeter candidates with the seeded point-of-interest random source.
  void ShuffleInPlace(List<Vector2Int> cells)
  {
    for (int i = cells.Count - 1; i > 0; i--)
    {
      int swapIndex = poiRandom.Next(i + 1);
      (cells[i], cells[swapIndex]) = (cells[swapIndex], cells[i]);
    }
  }

  // Instantiates the configured portal or a placeholder and parents it to generated output.
  GameObject SpawnPortal(Vector3 worldPosition, Quaternion rotation)
  {
    GameObject instance = poiSet.spawnPortalPrefab != null
      ? Instantiate(poiSet.spawnPortalPrefab, worldPosition, rotation)
      : CreatePlaceholder(poiSet.spawnPortalColour, worldPosition);

    instance.name = "Spawn Portal";
    instance.transform.SetParent(generatedRoot, true);
    return instance;
  }

  // Places one seeded loot variant at the room centre.
  void PlaceLoot(DungeonRoomInfo room, int gridWidth, int gridHeight)
  {
    Color placeholderColor = poiSet.lootColour;
    string label = "Loot";

    Vector3 worldPosition = tilePlacer.GridToWorld(room.Center, gridWidth, gridHeight);
    SpawnFromSet(poiSet.lootPrefabs, placeholderColor, worldPosition, Quaternion.identity, label);
  }

  // Detaches and rebinds the player after the spawn portal has been instantiated.
  void PlacePlayer(GameObject portalInstance)
  {
    if (player == null)
    {
      ResolvePlayerFromPortal(portalInstance);
    }

    if (player == null)
    {
      Debug.LogWarning($"{nameof(DungeonPointOfInterestPlacer3D)} has no player assigned - skipping spawn placement.");
      return;
    }

    // Preserve the Player's authored world pose while removing it from the portal hierarchy.
    if (player.parent != null)
    {
      player.SetParent(null, true);
    }

    player.localScale = Vector3.one;

    RefreshPlayerRuntimeReferences();

    // Adopt the Player's authored facing before locomotion starts following the camera.
    playerCameraOrbit?.SnapImmediatelyToTarget(player);
  }

  // Searches the portal hierarchy for the player when no runtime player was already found.
  void ResolvePlayerFromPortal(GameObject portalInstance)
  {
    if (portalInstance == null)
    {
      return;
    }

    Transform portalPlayer = FindTaggedChild(portalInstance.transform, "Player");
    if (portalPlayer == null)
    {
      PlayerLocomotion locomotion = portalInstance.GetComponentInChildren<PlayerLocomotion>(true);
      portalPlayer = locomotion != null ? locomotion.transform : null;
    }

    if (portalPlayer == null)
    {
      return;
    }

    player = portalPlayer;
    playerCameraOrbit = player.GetComponent<PlayerCameraOrbit>();
  }

  // Refreshes movement, lock-on, and camera references after detaching the player.
  void RefreshPlayerRuntimeReferences()
  {
    if (player == null)
    {
      return;
    }

    PlayerLocomotion locomotion = player.GetComponent<PlayerLocomotion>();
    locomotion?.RefreshRuntimeReferences();

    TargetLockController lockController = player.GetComponent<TargetLockController>();
    lockController?.RefreshRuntimeReferences();

    playerCameraOrbit = player.GetComponent<PlayerCameraOrbit>();
    playerCameraOrbit?.RefreshRuntimeReferences();

    PlayerLockOnCamera lockOnCamera = player.GetComponent<PlayerLockOnCamera>();
    lockOnCamera?.RefreshRuntimeReferences();

    PlayerDeathCamera deathCamera = FindFirstObjectByType<PlayerDeathCamera>();
    deathCamera?.RefreshRuntimeReferences();
  }

  // Recursively searches a hierarchy for a child with the requested name.
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

  // Recursively searches a hierarchy for a child with the requested tag.
  static Transform FindTaggedChild(Transform root, string tag)
  {
    if (root == null)
    {
      return null;
    }

    foreach (Transform child in root)
    {
      if (child.CompareTag(tag))
      {
        return child;
      }

      Transform match = FindTaggedChild(child, tag);
      if (match != null)
      {
        return match;
      }
    }

    return null;
  }

  // Selects and instantiates a seeded prefab variant or a placeholder point of interest.
  void SpawnFromSet(GameObject[] variants, Color placeholderColor, Vector3 worldPosition, Quaternion rotation, string label)
  {
    GameObject prefab = variants == null || variants.Length == 0 ? null : variants[poiRandom.Next(variants.Length)];
    GameObject instance = prefab != null
      ? Instantiate(prefab, worldPosition, rotation)
      : CreatePlaceholder(placeholderColor, worldPosition);

    instance.name = label;
    instance.transform.SetParent(generatedRoot, false);
  }

  // Creates a coloured sphere placeholder for an unassigned point-of-interest prefab.
  GameObject CreatePlaceholder(Color color, Vector3 position)
  {
    GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    placeholder.transform.position = position + (Vector3.up * 0.5f);
    Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
    placeholder.GetComponent<Renderer>().sharedMaterial = new Material(shader) { color = color };
    return placeholder;
  }

  // Finds or creates the parent transform for generated points of interest.
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
  // Removes generated points of interest from the scene.
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
