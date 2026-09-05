using Unity.AI.Navigation;
using UnityEngine;

[RequireComponent(typeof(NavMeshSurface))]
public class DungeonNavMeshBaker : MonoBehaviour
{
  [SerializeField] NavMeshSurface surface;

  [ContextMenu("Generate")]
  public void Generate()
  {
    surface.BuildNavMesh();
  }

  [ContextMenu("Clear")]
  public void Clear()
  {
    surface?.RemoveData();
  }
}
