using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Creates the shared DeathCam prefab (top-down orbit vcam) and wires one instance into each
// gameplay scene, targeting that scene's Player and referencing its existing gameplay vcam/brain.
static class PlayerDeathCameraSetup
{
  const string DeathCamPrefabPath = "Assets/_Project/Prefabs/Systems/DeathCam.prefab";

  static readonly string[] GameplayScenePaths =
  {
    "Assets/_Project/Scenes/Dungeon.unity",
    "Assets/_Project/Scenes/Sandbox.unity",
  };

  [MenuItem("Tools/Project Setup/Configure Death Camera")]
  static void ConfigureDeathCamera()
  {
    GameObject deathCamPrefab = EnsureDeathCamPrefab();

    foreach (string path in GameplayScenePaths)
    {
      Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

      if (Object.FindFirstObjectByType<PlayerDeathCamera>() != null)
      {
        Debug.Log($"Death camera already configured in {path}; skipping.");
        continue;
      }

      PlayerLockOnCamera lockOnCamera = Object.FindFirstObjectByType<PlayerLockOnCamera>();
      CinemachineBrain brain = Object.FindFirstObjectByType<CinemachineBrain>();

      if (lockOnCamera == null || brain == null)
      {
        Debug.LogWarning($"PlayerDeathCameraSetup: could not find PlayerLockOnCamera/CinemachineBrain in {path}; skipping.");
        continue;
      }

      var serializedLockOnCamera = new SerializedObject(lockOnCamera);
      CinemachineCamera gameplayVcam = serializedLockOnCamera.FindProperty("orbitalVcam").objectReferenceValue as CinemachineCamera;

      if (gameplayVcam == null)
      {
        Debug.LogWarning($"PlayerDeathCameraSetup: PlayerLockOnCamera has no orbitalVcam assigned in {path}; skipping.");
        continue;
      }

      Transform player = lockOnCamera.transform;

      var deathCamInstance = (GameObject)PrefabUtility.InstantiatePrefab(deathCamPrefab, scene);
      CinemachineCamera deathVcam = deathCamInstance.GetComponent<CinemachineCamera>();
      deathVcam.Follow = player;
      deathVcam.LookAt = player;

      CinemachineDeoccluder gameplayDeoccluder = gameplayVcam.GetComponent<CinemachineDeoccluder>();
      CinemachineDeoccluder deathDeoccluder = deathCamInstance.GetComponent<CinemachineDeoccluder>();
      if (gameplayDeoccluder != null && deathDeoccluder != null)
      {
        deathDeoccluder.CollideAgainst = gameplayDeoccluder.CollideAgainst;
      }

      PlayerDeathCamera deathCameraController = deathCamInstance.GetComponent<PlayerDeathCamera>();
      var serializedController = new SerializedObject(deathCameraController);
      serializedController.FindProperty("gameplayVcam").objectReferenceValue = gameplayVcam;
      serializedController.FindProperty("brain").objectReferenceValue = brain;
      serializedController.ApplyModifiedProperties();

      EditorSceneManager.SaveScene(scene);
      Debug.Log($"Death camera configured in {path}.");
    }
  }

  static GameObject EnsureDeathCamPrefab()
  {
    GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(DeathCamPrefabPath);
    if (existing != null)
    {
      return existing;
    }

    var go = new GameObject("DeathCam", typeof(CinemachineCamera), typeof(CinemachineOrbitalFollow),
      typeof(CinemachineHardLookAt), typeof(CinemachineDeoccluder), typeof(PlayerDeathCamera));

    CinemachineOrbitalFollow orbitalFollow = go.GetComponent<CinemachineOrbitalFollow>();
    orbitalFollow.OrbitStyle = CinemachineOrbitalFollow.OrbitStyles.Sphere;
    orbitalFollow.Radius = 18f;
    orbitalFollow.TrackerSettings.BindingMode = Unity.Cinemachine.TargetTracking.BindingMode.WorldSpace;

    orbitalFollow.HorizontalAxis.Value = 0f;
    orbitalFollow.HorizontalAxis.Center = 0f;
    orbitalFollow.HorizontalAxis.Range = new Vector2(-180f, 180f);
    orbitalFollow.HorizontalAxis.Wrap = true;

    orbitalFollow.VerticalAxis.Value = 70f;
    orbitalFollow.VerticalAxis.Center = 70f;
    orbitalFollow.VerticalAxis.Range = new Vector2(10f, 85f);

    go.GetComponent<CinemachineDeoccluder>().IgnoreTag = "Player";

    GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, DeathCamPrefabPath);
    Object.DestroyImmediate(go);
    return prefab;
  }
}
