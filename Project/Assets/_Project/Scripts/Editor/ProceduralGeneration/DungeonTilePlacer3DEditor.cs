#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DungeonTilePlacer3D))]
// Adds generation controls to the DungeonTilePlacer3D inspector.
public class DungeonTilePlacer3DEditor : Editor
{
  // Draws serialized placement settings and dispatches generation or cleanup actions.
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    GUILayout.Space(10f);

    DungeonTilePlacer3D placer = (DungeonTilePlacer3D)target;

    // Generates tiles from the current 2D metadata and marks the component dirty.
    if (GUILayout.Button("Generate"))
    {
      placer.Generate();
      EditorUtility.SetDirty(placer);
    }

    // Removes generated tiles and marks the component dirty.
    if (GUILayout.Button("Clear"))
    {
      placer.Clear();
      EditorUtility.SetDirty(placer);
    }
  }
}
#endif
