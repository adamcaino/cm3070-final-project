using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Wires the lock-on HUD and player SFX into the UI scene and player prefab.
static class LockOnFeedbackSetup
{
  const string UIScenePath = "Assets/_Project/Scenes/UI.unity";
  const string PlayerPrefabPath = "Assets/_Project/Prefabs/Characters/Player.prefab";
  const string WhiteDotPrefabPath = "Assets/_Project/Prefabs/UI/Alert_Dot_White.prefab";
  const string RedDotPrefabPath = "Assets/_Project/Prefabs/UI/Alert_Dot_Red.prefab";
  const string ToggleLockOnClipPath = "Assets/_Project/Audio/SFX/Player/Player_Toggle_LockOn.wav";

  [MenuItem("Tools/Project Setup/Configure Lock-On Feedback")]
  static void ConfigureLockOnFeedback()
  {
    ConfigureIcons();
    ConfigurePlayerAudio();
  }

  static void ConfigureIcons()
  {
    Scene scene = EditorSceneManager.OpenScene(UIScenePath, OpenSceneMode.Single);

    if (Object.FindFirstObjectByType<TargetLockIconController>() != null)
    {
      Debug.Log("Lock-on icons already configured in UI scene; skipping.");
      return;
    }

    Canvas canvas = Object.FindFirstObjectByType<Canvas>();
    if (canvas == null)
    {
      Debug.LogError("LockOnFeedbackSetup: no Canvas found in UI scene. Run 'Configure UI Scene' first.");
      return;
    }

    var root = new GameObject("TargetLockIcons", typeof(RectTransform));
    root.transform.SetParent(canvas.transform, false);

    RectTransform rect = root.GetComponent<RectTransform>();
    rect.anchorMin = Vector2.zero;
    rect.anchorMax = Vector2.one;
    rect.offsetMin = Vector2.zero;
    rect.offsetMax = Vector2.zero;

    TargetLockIconController controller = root.AddComponent<TargetLockIconController>();

    GameObject whitePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(WhiteDotPrefabPath);
    GameObject redPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RedDotPrefabPath);

    var serializedController = new SerializedObject(controller);
    serializedController.FindProperty("whiteIconPrefab").objectReferenceValue = whitePrefab;
    serializedController.FindProperty("redIconPrefab").objectReferenceValue = redPrefab;
    serializedController.ApplyModifiedProperties();

    EditorSceneManager.SaveScene(scene);
    Debug.Log("Lock-on icons configured in UI scene: TargetLockIcons wired up with Alert_Dot prefabs.");
  }

  static void ConfigurePlayerAudio()
  {
    GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);

    if (prefabRoot.GetComponent<PlayerLockOnAudio>() != null)
    {
      Debug.Log("Player lock-on audio already configured; skipping.");
      PrefabUtility.UnloadPrefabContents(prefabRoot);
      return;
    }

    if (prefabRoot.GetComponent<TargetLockController>() == null || prefabRoot.GetComponent<AudioSource>() == null)
    {
      Debug.LogError("LockOnFeedbackSetup: Player prefab is missing TargetLockController or AudioSource.");
      PrefabUtility.UnloadPrefabContents(prefabRoot);
      return;
    }

    PlayerLockOnAudio audio = prefabRoot.AddComponent<PlayerLockOnAudio>();
    AudioClip toggleClip = AssetDatabase.LoadAssetAtPath<AudioClip>(ToggleLockOnClipPath);

    var serializedAudio = new SerializedObject(audio);
    serializedAudio.FindProperty("toggleLockOnClip").objectReferenceValue = toggleClip;
    serializedAudio.ApplyModifiedProperties();

    PrefabUtility.SaveAsPrefabAsset(prefabRoot, PlayerPrefabPath);
    PrefabUtility.UnloadPrefabContents(prefabRoot);

    Debug.Log("Player lock-on audio configured: PlayerLockOnAudio added with Player_Toggle_LockOn.wav.");
  }
}
