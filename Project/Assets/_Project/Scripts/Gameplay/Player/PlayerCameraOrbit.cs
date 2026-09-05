using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

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

  void Awake()
  {
    RefreshRuntimeReferences();
  }

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

  void OnEnable()
  {
    lookAction?.action.Enable();
    PlayerDiedSignal.Raised += HandlePlayerDied;
    GameOverSignal.VictoryRaised += HandleVictoryRaised;
  }

  void OnDisable()
  {
    lookAction?.action.Disable();
    PlayerDiedSignal.Raised -= HandlePlayerDied;
    GameOverSignal.VictoryRaised -= HandleVictoryRaised;
  }

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

  void HandlePlayerDied()
  {
    isDead = true;

    if (inputAxisController != null)
    {
      inputAxisController.enabled = false;
    }

    lookAction?.action.Disable();
  }

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

  public static float GetSensitivity()
  {
    return PlayerPrefs.GetFloat(SensitivityPlayerPrefsKey, DefaultSensitivity);
  }

  public static void SetSensitivity(float value)
  {
    PlayerPrefs.SetFloat(SensitivityPlayerPrefsKey, Mathf.Clamp(value, 0.1f, 2f));
    PlayerPrefs.Save();
  }

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

  public void SnapToTarget(Transform target)
  {
    if (orbitalFollow == null || target == null)
    {
      return;
    }

    orbitalFollow.HorizontalAxis.Value = orbitalFollow.HorizontalAxis.ClampValue(target.eulerAngles.y);
    orbitalFollow.VerticalAxis.Value = orbitalFollow.VerticalAxis.ClampValue(orbitalFollow.VerticalAxis.Center);
  }

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
