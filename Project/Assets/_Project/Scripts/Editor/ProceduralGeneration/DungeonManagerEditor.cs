#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DungeonManager))]
// Adds comparison controls to the archived dungeon manager inspector.
public class DungeonManagerEditor : Editor
{
  // Draws comparison settings and dispatches selected, combined, or clear actions.
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    GUILayout.Space(10f);

    DungeonManager dungeonManager = (DungeonManager)target;

    // Generates the selected algorithm using the configured seed policy.
    if (GUILayout.Button("Generate Selected Layout"))
    {
      dungeonManager.GenerateSelectedLayout();
      EditorUtility.SetDirty(dungeonManager);
    }

    // Generates both algorithms with the same comparison seed when configured.
    if (GUILayout.Button("Generate Both Layouts"))
    {
      dungeonManager.GenerateBothLayouts();
      EditorUtility.SetDirty(dungeonManager);
    }

    // Clears output from both comparison generators.
    if (GUILayout.Button("Clear All Layouts"))
    {
      dungeonManager.ClearAllLayouts();
      EditorUtility.SetDirty(dungeonManager);
    }
  }
}
#endif
