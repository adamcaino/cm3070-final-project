using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

// Applies look input to the gameplay camera and provides runtime target-snap utilities.
public class PlayerCameraOrbit : MonoBehaviour
{
  public const string SensitivityPlayerPrefsKey = "MouseSensitivity";
  public const float DefaultSensitivity = 0.25f;

  [Header("Input")]
  [SerializeField] InputActionReference lookAction;

  [Header("Target")]
  [SerializeField] CinemachineOrbitalFollow orbitalFollow;

  [Header("Sensitivity")]
  [SerializeField] float horizontalSpeed = 1f;
  [SerializeField] float verticalSpeed = 1f;
  [SerializeField] bool invertVertical;

  bool isDead;
  CinemachineInputAxisController inputAxisController;
  Vector3 spawnSnapDefaultPositionDamping;
  bool hasSpawnSnapDefaultPositionDamping;
  Coroutine restoreSpawnSnapDampingRoutine;

  // Resolves the gameplay orbital follow during initialization.
  void Awake()
  {
    RefreshRuntimeReferences();
  }

  // Resolves camera components, configures tracking, and refreshes camera targets.
  public void RefreshRuntimeReferences()
  {
    if (orbitalFollow == null)
    {
      orbitalFollow = ResolveGameplayOrbitalFollow();
    }

    if (orbitalFollow != null)
    {
      orbitalFollow.TrackerSettings.BindingMode = Unity.Cinemachine.TargetTracking.BindingMode.WorldSpace;
      if (!hasSpawnSnapDefaultPositionDamping)
      {
        spawnSnapDefaultPositionDamping = orbitalFollow.TrackerSettings.PositionDamping;
        hasSpawnSnapDefaultPositionDamping = true;
      }

      inputAxisController = orbitalFollow.GetComponent<CinemachineInputAxisController>();

      CinemachineCamera camera = orbitalFollow.GetComponent<CinemachineCamera>();
      if (camera != null)
      {
        Transform cameraTarget = transform.Find("CameraTargetPos");
        camera.Follow = cameraTarget;
        camera.LookAt = cameraTarget;
        camera.PreviousStateIsValid = false;
      }
    }
  }

  // Finds the orbital follow that is not assigned to the player death camera.
  public static CinemachineOrbitalFollow ResolveGameplayOrbitalFollow()
  {
    CinemachineOrbitalFollow[] orbitalFollows = FindObjectsByType<CinemachineOrbitalFollow>(FindObjectsSortMode.None);
    foreach (CinemachineOrbitalFollow candidate in orbitalFollows)
    {
      if (candidate.GetComponent<PlayerDeathCamera>() == null)
      {
        return candidate;
      }
    }

    return null;
  }

  // Enables look input and subscribes to death and victory signals.
  void OnEnable()
  {
    lookAction?.action.Enable();
    PlayerDiedSignal.Raised += HandlePlayerDied;
    GameOverSignal.VictoryRaised += HandleVictoryRaised;
  }

  // Disables look input and removes death and victory subscriptions.
  void OnDisable()
  {
    lookAction?.action.Disable();
    PlayerDiedSignal.Raised -= HandlePlayerDied;
    GameOverSignal.VictoryRaised -= HandleVictoryRaised;
  }

  // Applies horizontal and vertical look input to the orbital camera axes.
  void Update()
  {
    if (isDead || orbitalFollow == null || lookAction == null)
    {
      return;
    }

    Vector2 lookInput = lookAction.action.ReadValue<Vector2>();
    float verticalSign = invertVertical ? 1f : -1f;

    orbitalFollow.HorizontalAxis.Value = orbitalFollow.HorizontalAxis.ClampValue(
      orbitalFollow.HorizontalAxis.Value + (lookInput.x * horizontalSpeed * GetSensitivity()));

    orbitalFollow.VerticalAxis.Value = orbitalFollow.VerticalAxis.ClampValue(
      orbitalFollow.VerticalAxis.Value + (lookInput.y * verticalSpeed * verticalSign * GetSensitivity()));
  }

  // Disables camera input and the Cinemachine input controller after player death.
  void HandlePlayerDied()
  {
    isDead = true;

    if (inputAxisController != null)
    {
      inputAxisController.enabled = false;
    }

    lookAction?.action.Disable();
  }

  // Disables camera input when victory belongs to this scene.
  void HandleVictoryRaised(string sceneName)
  {
    if (sceneName != gameObject.scene.name)
    {
      return;
    }

    isDead = true;

    if (inputAxisController != null)
    {
      inputAxisController.enabled = false;
    }

    lookAction?.action.Disable();
  }

  // Reads the saved mouse sensitivity or returns the default value.
  public static float GetSensitivity()
  {
    return PlayerPrefs.GetFloat(SensitivityPlayerPrefsKey, DefaultSensitivity);
  }

  // Clamps and saves the mouse sensitivity value.
  public static void SetSensitivity(float value)
  {
    PlayerPrefs.SetFloat(SensitivityPlayerPrefsKey, Mathf.Clamp(value, 0.1f, 2f));
    PlayerPrefs.Save();
  }

  // Aligns the camera's horizontal axis with the target's yaw.
  public void SnapYawToTarget(Transform target)
  {
    if (orbitalFollow == null || target == null)
    {
      return;
    }

    float yaw = target.eulerAngles.y;
    if (yaw > 180f)
    {
      yaw -= 360f;
    }

    orbitalFollow.HorizontalAxis.Value = orbitalFollow.HorizontalAxis.ClampValue(yaw);
  }

  // Aligns horizontal and vertical camera axes with the target.
  public void SnapToTarget(Transform target)
  {
    if (orbitalFollow == null || target == null)
    {
      return;
    }

    orbitalFollow.HorizontalAxis.Value = orbitalFollow.HorizontalAxis.ClampValue(target.eulerAngles.y);
    orbitalFollow.VerticalAxis.Value = orbitalFollow.VerticalAxis.ClampValue(orbitalFollow.VerticalAxis.Center);
  }

  // Snaps to a target and temporarily removes position damping to prevent camera drift.
  public void SnapImmediatelyToTarget(Transform target)
  {
    if (orbitalFollow == null || target == null)
    {
      return;
    }

    if (!hasSpawnSnapDefaultPositionDamping)
    {
      spawnSnapDefaultPositionDamping = orbitalFollow.TrackerSettings.PositionDamping;
      hasSpawnSnapDefaultPositionDamping = true;
    }

    SnapToTarget(target);

    var trackerSettings = orbitalFollow.TrackerSettings;
    trackerSettings.PositionDamping = Vector3.zero;
    orbitalFollow.TrackerSettings = trackerSettings;

    CinemachineCamera camera = orbitalFollow.GetComponent<CinemachineCamera>();
    if (camera != null)
    {
      camera.PreviousStateIsValid = false;
    }

    if (restoreSpawnSnapDampingRoutine != null)
    {
      StopCoroutine(restoreSpawnSnapDampingRoutine);
    }

    restoreSpawnSnapDampingRoutine = StartCoroutine(RestoreSpawnSnapDampingNextFrame());
  }

  // Restores the camera's authored position damping on the frame after a spawn snap.
  System.Collections.IEnumerator RestoreSpawnSnapDampingNextFrame()
  {
    yield return null;

    if (orbitalFollow != null && hasSpawnSnapDefaultPositionDamping)
    {
      var trackerSettings = orbitalFollow.TrackerSettings;
      trackerSettings.PositionDamping = spawnSnapDefaultPositionDamping;
      orbitalFollow.TrackerSettings = trackerSettings;
    }

    restoreSpawnSnapDampingRoutine = null;
  }
}
