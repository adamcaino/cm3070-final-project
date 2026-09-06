using UnityEngine;

// Small orchestration component for comparing the procedural generation proof of concepts.
// It can run either algorithm independently or both using the same seed.
public class DungeonManager : MonoBehaviour
{
  // Selects which archived generation algorithm the comparison controls invoke.
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
  // Generates the selected algorithm using either the shared or a fresh seed.
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
  // Generates both archived algorithms with the same comparison seed when configured.
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
  // Clears generated output from both archived algorithms.
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
