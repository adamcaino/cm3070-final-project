using Unity.AI.Navigation;
using UnityEngine;

public class DungeonNavMeshBaker : MonoBehaviour
{
  [SerializeField] NavMeshSurface surface;

  [ContextMenu("Generate")]
  public void Generate()
  {
    if (surface == null)
    {
      Debug.LogWarning($"{nameof(DungeonNavMeshBaker)} is missing a NavMeshSurface.");
      return;
    }

    surface.BuildNavMesh();
  }

  [ContextMenu("Clear")]
  public void Clear()
  {
    surface?.RemoveData();
  }
}
