#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DungeonLevelGenerator))]
public class DungeonLevelGeneratorEditor : Editor
{
  // Adds quick generation and cleanup controls for dungeon layout generation.
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    GUILayout.Space(10f);

    DungeonLevelGenerator generator = (DungeonLevelGenerator)target;

    if (GUILayout.Button("Generate"))
    {
      generator.Generate();
      EditorUtility.SetDirty(generator);
    }

    if (GUILayout.Button("Clear"))
    {
      generator.Clear();
      EditorUtility.SetDirty(generator);
    }
  }
}
#endif
