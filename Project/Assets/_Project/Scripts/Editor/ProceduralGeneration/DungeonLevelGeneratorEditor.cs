#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DungeonLevelGenerator))]
// Adds generation controls to the full DungeonLevelGenerator inspector.
public class DungeonLevelGeneratorEditor : Editor
{
  // Draws pipeline references and dispatches generation or cleanup actions.
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    GUILayout.Space(10f);

    DungeonLevelGenerator generator = (DungeonLevelGenerator)target;

    // Starts the full staged generation pipeline and marks the component dirty.
    if (GUILayout.Button("Generate"))
    {
      generator.Generate();
      EditorUtility.SetDirty(generator);
    }

    // Clears every generated pipeline stage and marks the component dirty.
    if (GUILayout.Button("Clear"))
    {
      generator.Clear();
      EditorUtility.SetDirty(generator);
    }
  }
}
#endif
