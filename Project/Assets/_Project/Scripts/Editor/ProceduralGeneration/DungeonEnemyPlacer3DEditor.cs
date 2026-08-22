#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DungeonEnemyPlacer3D))]
public class DungeonEnemyPlacer3DEditor : Editor
{
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    GUILayout.Space(10f);

    DungeonEnemyPlacer3D placer = (DungeonEnemyPlacer3D)target;

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
