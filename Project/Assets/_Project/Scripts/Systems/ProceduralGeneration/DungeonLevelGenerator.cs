using System.Collections;
using UnityEngine;

/// <summary>
/// Single entry point for the full level-generation pipeline: 2D BSP layout, then 3D tile placement,
/// then prop scattering, each stage reading the previous stage's output. Always regenerates on Start
/// (using PendingSeed when set by a scene reload, otherwise a fresh random seed) so the NavMesh and
/// level geometry are baked fresh for every play session rather than relying on an editor-time bake,
/// which is not persisted and does not survive entering Play mode. The 2D layout and the 3D level are
/// generated under their own generators' transforms, so keeping those two generators apart in the scene
/// (e.g. the 2D one parked away from play space to double as a future minimap source) keeps their
/// output apart too.
/// </summary>
public class DungeonLevelGenerator : MonoBehaviour
{
  [SerializeField] DungeonGridGenerator2D gridGenerator;
  [SerializeField] DungeonTilePlacer3D tilePlacer;
  [SerializeField] DungeonPropPlacer3D propPlacer;
  [SerializeField] DungeonNavMeshBaker navMeshBaker;
  [SerializeField] DungeonPointOfInterestPlacer3D poiPlacer;
  [SerializeField] DungeonBossPlacer3D bossPlacer;
  [SerializeField] DungeonEnemyPlacer3D enemyPlacer;

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
  public void Generate()
  {
    if (!HasAllReferences())
    {
      return;
    }

    StartCoroutine(GenerateSequence());
  }

  public void Generate(int seed)
  {
    if (!HasAllReferences())
    {
      return;
    }

    StartCoroutine(GenerateSequence(seed));
  }

  IEnumerator GenerateSequence(int? seed = null)
  {
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
