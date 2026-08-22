using UnityEngine;

/// <summary>
/// Single entry point for the full level-generation pipeline: 2D BSP layout, then 3D tile placement,
/// then prop scattering, each stage reading the previous stage's output. Exists so the editor button
/// (and eventually a runtime level-load call) only has to drive one method instead of three separate
/// components. The 2D layout and the 3D level are generated under their own generators' transforms, so
/// keeping those two generators apart in the scene (e.g. the 2D one parked away from play space to
/// double as a future minimap source) keeps their output apart too.
/// </summary>
public class DungeonLevelGenerator : MonoBehaviour
{
  [SerializeField] DungeonGridGenerator2D gridGenerator;
  [SerializeField] DungeonTilePlacer3D tilePlacer;
  [SerializeField] DungeonPropPlacer3D propPlacer;
  [SerializeField] DungeonNavMeshBaker navMeshBaker;
  [SerializeField] DungeonPointOfInterestPlacer3D poiPlacer;
  [SerializeField] DungeonEnemyPlacer3D enemyPlacer;

  [Header("Reproducibility")]
  [SerializeField] bool generateOnStart;

  void Start()
  {
    if (generateOnStart)
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

    gridGenerator.Generate();
    tilePlacer.Generate();
    propPlacer.Generate();
    navMeshBaker.Generate();
    poiPlacer.Generate();
    enemyPlacer.Generate();
  }

  public void Generate(int seed)
  {
    if (!HasAllReferences())
    {
      return;
    }

    gridGenerator.Generate(seed);
    tilePlacer.Generate();
    propPlacer.Generate();
    navMeshBaker.Generate();
    poiPlacer.Generate();
    enemyPlacer.Generate();
  }

  [ContextMenu("Clear")]
  public void Clear()
  {
    enemyPlacer?.Clear();
    poiPlacer?.Clear();
    navMeshBaker?.Clear();
    propPlacer?.Clear();
    tilePlacer?.Clear();
    gridGenerator?.Clear();
  }

  bool HasAllReferences()
  {
    if (gridGenerator != null && tilePlacer != null && propPlacer != null && navMeshBaker != null && poiPlacer != null && enemyPlacer != null)
    {
      return true;
    }

    Debug.LogWarning($"{nameof(DungeonLevelGenerator)} is missing a grid generator, tile placer, prop placer, navmesh baker, point of interest placer, or enemy placer.");
    return false;
  }
}
