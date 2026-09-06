#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DungeonPointOfInterestPlacer3D))]
// Adds generation controls to the DungeonPointOfInterestPlacer3D inspector.
public class DungeonPointOfInterestPlacer3DEditor : Editor
{
  // Draws serialized point-of-interest settings and dispatches generation or cleanup actions.
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    GUILayout.Space(10f);

    DungeonPointOfInterestPlacer3D placer = (DungeonPointOfInterestPlacer3D)target;

    // Generates spawn and loot points and marks the component dirty.
    if (GUILayout.Button("Generate"))
    {
      placer.Generate();
      EditorUtility.SetDirty(placer);
    }

    // Removes generated points and marks the component dirty.
    if (GUILayout.Button("Clear"))
    {
      placer.Clear();
      EditorUtility.SetDirty(placer);
    }
  }
}
#endif
