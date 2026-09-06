using Unity.AI.Navigation;
using UnityEngine;

[RequireComponent(typeof(NavMeshSurface))]
// Builds and removes the NavMesh used by the generated dungeon geometry.
public class DungeonNavMeshBaker : MonoBehaviour
{
  [SerializeField] NavMeshSurface surface;

  [ContextMenu("Generate")]
  // Builds navigation data from the currently generated tile geometry.
  public void Generate()
  {
    surface.BuildNavMesh();
  }

  [ContextMenu("Clear")]
  // Removes the generated navigation data from the surface.
  public void Clear()
  {
    surface?.RemoveData();
  }
}
