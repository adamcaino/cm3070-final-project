using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

// Adds and wires PlayerDeathHandler on the Player prefab.
static class PlayerDeathSetup
{
  const string PlayerPrefabPath = "Assets/_Project/Prefabs/Characters/Player.prefab";
  const string InputActionsPath = "Assets/_Project/Settings/PlayerControls.inputactions";

  [MenuItem("Tools/Project Setup/Configure Player Death")]
  static void ConfigurePlayerDeath()
  {
    GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);

    PlayerDeathHandler handler = prefabRoot.GetComponent<PlayerDeathHandler>();
    if (handler == null)
    {
      handler = prefabRoot.AddComponent<PlayerDeathHandler>();
    }

    InputActionAsset playerControls = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
    if (playerControls == null)
    {
      Debug.LogWarning($"PlayerDeathSetup: could not load InputActionAsset at {InputActionsPath}.");
    }

    var serializedHandler = new SerializedObject(handler);
    serializedHandler.FindProperty("playerControls").objectReferenceValue = playerControls;
    serializedHandler.ApplyModifiedProperties();

    PrefabUtility.SaveAsPrefabAsset(prefabRoot, PlayerPrefabPath);
    PrefabUtility.UnloadPrefabContents(prefabRoot);

    Debug.Log("Player prefab configured: PlayerDeathHandler added and wired.");
  }
}
