using System.Collections;
using System.Diagnostics;
using Unity.Profiling;
using UnityEngine;
using Debug = UnityEngine.Debug;

// Runs the dungeon pipeline from seeded 2D layout through 3D placement, NavMesh baking, and combat setup.
// Each stage consumes the output produced by the preceding stage.
public class DungeonLevelGenerator : MonoBehaviour
{
  static readonly ProfilerMarker GridGenerationMarker = new ProfilerMarker("Dungeon.Generation.Grid");
  static readonly ProfilerMarker TilePlacementMarker = new ProfilerMarker("Dungeon.Generation.Tiles");
  static readonly ProfilerMarker PropPlacementMarker = new ProfilerMarker("Dungeon.Generation.Props");
  static readonly ProfilerMarker NavMeshBuildMarker = new ProfilerMarker("Dungeon.Generation.NavMesh");
  static readonly ProfilerMarker PointOfInterestPlacementMarker = new ProfilerMarker("Dungeon.Generation.PointsOfInterest");
  static readonly ProfilerMarker BossPlacementMarker = new ProfilerMarker("Dungeon.Generation.Boss");
  static readonly ProfilerMarker EnemyPlacementMarker = new ProfilerMarker("Dungeon.Generation.Enemies");

  [SerializeField] DungeonGridGenerator2D gridGenerator;
  [SerializeField] DungeonTilePlacer3D tilePlacer;
  [SerializeField] DungeonPropPlacer3D propPlacer;
  [SerializeField] DungeonNavMeshBaker navMeshBaker;
  [SerializeField] DungeonPointOfInterestPlacer3D poiPlacer;
  [SerializeField] DungeonBossPlacer3D bossPlacer;
  [SerializeField] DungeonEnemyPlacer3D enemyPlacer;

  // Consumes a pending seed when present, otherwise starts a new random generation.
  void Start()
  {
    if (PendingSeed.Consume(out int seed))
    {
      Generate(seed);
    }
    else
    {
      Generate();
    }
  }

  [ContextMenu("Generate")]
  // Starts the asynchronous generation pipeline using the generator's normal seed rules.
  public void Generate()
  {
    if (!HasAllReferences())
    {
      return;
    }

    StartCoroutine(GenerateSequence());
  }

  // Starts the asynchronous generation pipeline with an explicit seed.
  public void Generate(int seed)
  {
    if (!HasAllReferences())
    {
      return;
    }

    StartCoroutine(GenerateSequence(seed));
  }

  // Runs the dependent generation stages in order and raises readiness after enemy placement.
  IEnumerator GenerateSequence(int? seed = null)
  {
    Stopwatch totalGenerationTime = Stopwatch.StartNew();
    DungeonReadySignal.Reset();

    using (GridGenerationMarker.Auto())
    {
      if (seed.HasValue)
      {
        gridGenerator.Generate(seed.Value);
      }
      else
      {
        gridGenerator.Generate();
      }
    }

    yield return null;
    using (TilePlacementMarker.Auto())
    {
      tilePlacer.Generate();
    }

    yield return null;
    using (PropPlacementMarker.Auto())
    {
      propPlacer.Generate();
    }

    yield return null;
    using (NavMeshBuildMarker.Auto())
    {
      navMeshBaker.Generate();
    }

    yield return null;
    using (PointOfInterestPlacementMarker.Auto())
    {
      poiPlacer.Generate();
    }

    yield return null;
    using (BossPlacementMarker.Auto())
    {
      bossPlacer.Generate();
    }

    yield return null;
    using (EnemyPlacementMarker.Auto())
    {
      enemyPlacer.Generate();
    }

    DungeonReadySignal.Raise();
    totalGenerationTime.Stop();
    Debug.Log($"Dungeon generation completed in {totalGenerationTime.Elapsed.TotalMilliseconds:F2} ms.");

    string roomCount = gridGenerator is BSPDungeonGenerator bspGenerator ? bspGenerator.LastRooms.Count.ToString() : "n/a";
    Debug.Log($"Dungeon summary - Rooms: {roomCount}, Regular enemies: {enemyPlacer.RegularEnemyCount}, Tough enemies: {enemyPlacer.ToughEnemyCount}, Bosses: {bossPlacer.BossCount}.");
  }

  [ContextMenu("Clear")]
  // Stops active generation and clears generated output in reverse dependency order.
  public void Clear()
  {
    StopAllCoroutines();
    enemyPlacer?.Clear();
    bossPlacer?.Clear();
    poiPlacer?.Clear();
    navMeshBaker?.Clear();
    propPlacer?.Clear();
    tilePlacer?.Clear();
    gridGenerator?.Clear();
  }

  // Verifies that every generation stage required by the pipeline is assigned.
  bool HasAllReferences()
  {
    if (gridGenerator != null && tilePlacer != null && propPlacer != null && navMeshBaker != null && poiPlacer != null && bossPlacer != null && enemyPlacer != null)
    {
      return true;
    }

    Debug.LogWarning($"{nameof(DungeonLevelGenerator)} is missing a grid generator, tile placer, prop placer, navmesh baker, point of interest placer, boss placer, or enemy placer.");
    return false;
  }
}
