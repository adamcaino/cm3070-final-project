#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DungeonPropPlacer3D))]
// Adds generation controls to the DungeonPropPlacer3D inspector.
public class DungeonPropPlacer3DEditor : Editor
{
  // Draws serialized prop settings and dispatches generation or cleanup actions.
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    GUILayout.Space(10f);

    DungeonPropPlacer3D placer = (DungeonPropPlacer3D)target;

    // Generates seeded wall props and marks the component dirty.
    if (GUILayout.Button("Generate"))
    {
      placer.Generate();
      EditorUtility.SetDirty(placer);
    }

    // Removes generated props and marks the component dirty.
    if (GUILayout.Button("Clear"))
    {
      placer.Clear();
      EditorUtility.SetDirty(placer);
    }
  }
}
#endif
