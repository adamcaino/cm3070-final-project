#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DungeonEnemyPlacer3D))]
// Adds generation controls to the DungeonEnemyPlacer3D inspector.
public class DungeonEnemyPlacer3DEditor : Editor
{
  // Draws serialized enemy settings and dispatches generation or cleanup actions.
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    GUILayout.Space(10f);

    DungeonEnemyPlacer3D placer = (DungeonEnemyPlacer3D)target;

    // Generates enemies from room metadata and marks the component dirty.
    if (GUILayout.Button("Generate"))
    {
      placer.Generate();
      EditorUtility.SetDirty(placer);
    }

    // Removes generated enemies and marks the component dirty.
    if (GUILayout.Button("Clear"))
    {
      placer.Clear();
      EditorUtility.SetDirty(placer);
    }
  }
}
#endif
