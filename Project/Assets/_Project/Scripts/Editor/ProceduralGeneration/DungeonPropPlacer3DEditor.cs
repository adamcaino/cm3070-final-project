#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DungeonPropPlacer3D))]
public class DungeonPropPlacer3DEditor : Editor
{
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    GUILayout.Space(10f);

    DungeonPropPlacer3D placer = (DungeonPropPlacer3D)target;

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
