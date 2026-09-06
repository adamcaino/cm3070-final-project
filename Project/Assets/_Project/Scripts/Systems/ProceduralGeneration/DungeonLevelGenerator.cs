using System.Collections;
using UnityEngine;

// Runs the dungeon pipeline from seeded 2D layout through 3D placement, NavMesh baking, and combat setup.
// Each stage consumes the output produced by the preceding stage.
public class DungeonLevelGenerator : MonoBehaviour
{
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
    DungeonReadySignal.Reset();

    if (seed.HasValue)
    {
      gridGenerator.Generate(seed.Value);
    }
    else
    {
      gridGenerator.Generate();
    }

    yield return null;
    tilePlacer.Generate();
    yield return null;
    propPlacer.Generate();
    yield return null;
    navMeshBaker.Generate();
    yield return null;
    poiPlacer.Generate();
    yield return null;
    bossPlacer.Generate();
    yield return null;
    enemyPlacer.Generate();

    DungeonReadySignal.Raise();
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
