using Unity.AI.Navigation;
using UnityEngine;

/// <summary>
/// Bakes the runtime NavMesh once the dungeon's geometry is final (after tiles and props, before enemies
/// or the player need to path on it). Thin wrapper around NavMeshSurface so this slots into
/// DungeonLevelGenerator's pipeline the same way the other placers do.
/// </summary>
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
