using UnityEngine;

/// <summary>
/// Small orchestration component for comparing the procedural generation proof of concepts.
/// It can run either algorithm independently or both using the same seed.
/// </summary>
public class DungeonManager : MonoBehaviour
{
  public enum GenerationAlgorithm
  {
    BSP,
    CellularAutomata
  }

  [Header("Comparison Mode")]
  [SerializeField] GenerationAlgorithm selectedAlgorithm = GenerationAlgorithm.BSP;
  [SerializeField] bool useSharedSeed = true;
  [SerializeField] int comparisonSeed = 3070;

  [Header("Generator References")]
  [SerializeField] BSPDungeonGenerator bspGenerator;
  [SerializeField] CellularAutomataDungeonGenerator cellularAutomataGenerator;

  [ContextMenu("Generate Selected Layout")]
  public void GenerateSelectedLayout()
  {
    int seed = useSharedSeed ? comparisonSeed : System.Environment.TickCount;

    switch (selectedAlgorithm)
    {
      case GenerationAlgorithm.BSP:
        if (bspGenerator != null)
        {
          bspGenerator.Generate(seed);
        }
        break;

      case GenerationAlgorithm.CellularAutomata:
        if (cellularAutomataGenerator != null)
        {
          cellularAutomataGenerator.Generate(seed);
        }
        break;
    }
  }

  [ContextMenu("Generate Both Layouts")]
  public void GenerateBothLayouts()
  {
    int seed = useSharedSeed ? comparisonSeed : System.Environment.TickCount;

    if (bspGenerator != null)
    {
      bspGenerator.Generate(seed);
    }

    if (cellularAutomataGenerator != null)
    {
      cellularAutomataGenerator.Generate(seed);
    }
  }

  [ContextMenu("Clear All Layouts")]
  public void ClearAllLayouts()
  {
    if (bspGenerator != null)
    {
      bspGenerator.Clear();
    }

    if (cellularAutomataGenerator != null)
    {
      cellularAutomataGenerator.Clear();
    }
  }
}
