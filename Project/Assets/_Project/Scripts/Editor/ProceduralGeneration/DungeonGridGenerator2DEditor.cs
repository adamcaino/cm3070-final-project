#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DungeonGridGenerator2D), true)]
public class DungeonGridGenerator2DEditor : Editor
{
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    GUILayout.Space(10f);

    DungeonGridGenerator2D generator = (DungeonGridGenerator2D)target;

    if (GUILayout.Button("Generate"))
    {
      generator.Generate();
      EditorUtility.SetDirty(generator);
    }

    if (GUILayout.Button("Regenerate Using Last Seed"))
    {
      generator.RegenerateUsingLastSeed();
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
