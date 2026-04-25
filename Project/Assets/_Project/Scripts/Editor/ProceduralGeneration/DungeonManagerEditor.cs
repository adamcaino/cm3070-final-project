#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DungeonManager))]
public class DungeonManagerEditor : Editor
{
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    GUILayout.Space(10f);

    DungeonManager dungeonManager = (DungeonManager)target;

    if (GUILayout.Button("Generate Selected Layout"))
    {
      dungeonManager.GenerateSelectedLayout();
      EditorUtility.SetDirty(dungeonManager);
    }

    if (GUILayout.Button("Generate Both Layouts"))
    {
      dungeonManager.GenerateBothLayouts();
      EditorUtility.SetDirty(dungeonManager);
    }

    if (GUILayout.Button("Clear All Layouts"))
    {
      dungeonManager.ClearAllLayouts();
      EditorUtility.SetDirty(dungeonManager);
    }
  }
}
#endif
