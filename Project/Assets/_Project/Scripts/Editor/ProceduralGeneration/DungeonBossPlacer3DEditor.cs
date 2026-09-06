#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DungeonBossPlacer3D))]
// Adds generation controls to the DungeonBossPlacer3D inspector.
public class DungeonBossPlacer3DEditor : Editor
{
  // Draws serialized boss settings and dispatches generation or cleanup actions.
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    GUILayout.Space(10f);

    DungeonBossPlacer3D placer = (DungeonBossPlacer3D)target;

    // Generates the boss encounter and marks the component dirty.
    if (GUILayout.Button("Generate"))
    {
      placer.Generate();
      EditorUtility.SetDirty(placer);
    }

    // Removes generated boss objects and marks the component dirty.
    if (GUILayout.Button("Clear"))
    {
      placer.Clear();
      EditorUtility.SetDirty(placer);
    }
  }
}
#endif
