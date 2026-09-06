#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DungeonGridGenerator2D), true)]
// Adds generation, seed reuse, and cleanup controls to 2D grid generator inspectors.
public class DungeonGridGenerator2DEditor : Editor
{
  // Draws grid settings and dispatches generation, seed reuse, or cleanup actions.
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    GUILayout.Space(10f);

    DungeonGridGenerator2D generator = (DungeonGridGenerator2D)target;

    // Generates a new map using the generator's configured seed policy.
    if (GUILayout.Button("Generate"))
    {
      generator.Generate();
      EditorUtility.SetDirty(generator);
    }

    // Rebuilds the map deterministically from the previous generation seed.
    if (GUILayout.Button("Regenerate Using Last Seed"))
    {
      generator.RegenerateUsingLastSeed();
      EditorUtility.SetDirty(generator);
    }

    // Removes the generated 2D map and marks the component dirty.
    if (GUILayout.Button("Clear"))
    {
      generator.Clear();
      EditorUtility.SetDirty(generator);
    }
  }
}
#endif
