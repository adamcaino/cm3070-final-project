#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DungeonPointOfInterestPlacer3D))]
public class DungeonPointOfInterestPlacer3DEditor : Editor
{
  // Adds generation and cleanup controls to the custom inspector.
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    GUILayout.Space(10f);

    DungeonPointOfInterestPlacer3D placer = (DungeonPointOfInterestPlacer3D)target;

    if (GUILayout.Button("Generate"))
    {
      placer.Generate();
      EditorUtility.SetDirty(placer);
    }

    if (GUILayout.Button("Clear"))
    {
      placer.Clear();
      EditorUtility.SetDirty(placer);
    }
  }
}
#endif
