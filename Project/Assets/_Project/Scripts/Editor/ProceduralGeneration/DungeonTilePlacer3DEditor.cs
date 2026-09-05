#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DungeonTilePlacer3D))]
public class DungeonTilePlacer3DEditor : Editor
{
  // Adds generation and cleanup controls to the custom inspector.
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    GUILayout.Space(10f);

    DungeonTilePlacer3D placer = (DungeonTilePlacer3D)target;

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
