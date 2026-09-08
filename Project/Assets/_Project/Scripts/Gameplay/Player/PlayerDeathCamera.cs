using Unity.Cinemachine;
using UnityEngine;

// Sits on the death-cam vcam itself. On player death, boosts its own priority above the gameplay
// vcam so CinemachineBrain blends into it (the pan/zoom out), then keeps it slowly auto-orbiting
// the corpse for as long as the Game Over screen is up - driving the same HorizontalAxis the player
// normally free-looks with, just from this script instead of input.
[RequireComponent(typeof(CinemachineCamera))]
[RequireComponent(typeof(CinemachineOrbitalFollow))]
public class PlayerDeathCamera : MonoBehaviour
{
  [Header("References")]
  [SerializeField] CinemachineCamera gameplayVcam;
  [SerializeField] CinemachineBrain brain;

  [Header("Blend")]
  [Tooltip("How long the pan/zoom out to the death cam takes.")]
  [SerializeField, Min(0.1f)] float blendDuration = 4f;
  [Tooltip("Added to the gameplay vcam's priority so the death cam always wins once active.")]
  [SerializeField] int activePriorityOffset = 100;

  [Header("Orbit")]
  [SerializeField, Min(0f)] float orbitSpeed = 6f;
  [SerializeField] float orbitPivotWorldY = 1.5f;

  CinemachineCamera deathVcam;
  CinemachineOrbitalFollow orbitalFollow;
  Transform playerTransform;
  Transform orbitPivot;
  bool isActive;

  // Caches the death camera components and resolves gameplay camera references.
  void Awake()
  {
    deathVcam = GetComponent<CinemachineCamera>();
    orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
    RefreshRuntimeReferences();
  }

  // Finds the gameplay camera and brain, sets startup priorities, and synchronizes targets.
  public void RefreshRuntimeReferences()
  {
    if (gameplayVcam == null)
    {
      CinemachineCamera[] cameras = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
      foreach (CinemachineCamera camera in cameras)
      {
        if (camera != null && camera != deathVcam && camera.GetComponent<PlayerDeathCamera>() == null)
        {
          gameplayVcam = camera;
          break;
        }
      }
    }

    if (brain == null)
    {
      brain = FindFirstObjectByType<CinemachineBrain>();
    }

    // Ensure DeathCam starts with lower priority than gameplay camera (which should be ~10).
    // On death, activePriorityOffset will push it above 100 to take over.
    if (deathVcam != null)
    {
      deathVcam.Priority = 0;
    }

    // Ensure gameplay camera has explicit priority higher than death cam at start.
    if (gameplayVcam != null && gameplayVcam.Priority.Value <= 0)
    {
      gameplayVcam.Priority = 10;
    }

    SyncPlayerTargets();
  }

  // Subscribes to the immediate player-death signal.
  void OnEnable()
  {
    PlayerDiedSignal.Raised += HandlePlayerDied;
  }

  // Removes the immediate player-death subscription.
  void OnDisable()
  {
    PlayerDiedSignal.Raised -= HandlePlayerDied;
  }

  // Cleans up the runtime pivot when this component is destroyed.
  void OnDestroy()
  {
    if (orbitPivot != null)
    {
      Destroy(orbitPivot.gameObject);
      orbitPivot = null;
    }
  }

  // Takes camera priority, copies gameplay orbit values, and starts the death view.
  void HandlePlayerDied()
  {
    SyncPlayerTargets();

    if (brain != null)
    {
      brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, blendDuration);
    }

    CinemachineCamera[] cameras = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
    foreach (CinemachineCamera camera in cameras)
    {
      if (camera != deathVcam)
      {
        camera.Priority = 0;
      }
    }

    deathVcam.Priority = Mathf.Max(activePriorityOffset, 1000);

    CinemachineOrbitalFollow gameplayOrbitalFollow = gameplayVcam != null
      ? gameplayVcam.GetComponent<CinemachineOrbitalFollow>()
      : null;

    if (gameplayOrbitalFollow != null)
    {
      orbitalFollow.Radius = gameplayOrbitalFollow.Radius;
      orbitalFollow.HorizontalAxis.Value = gameplayOrbitalFollow.HorizontalAxis.Value;
      orbitalFollow.VerticalAxis.Value = gameplayOrbitalFollow.VerticalAxis.Value;
    }

    deathVcam.PreviousStateIsValid = false;
    isActive = true;
  }

  // Assigns the player as the death camera's follow and look-at target.
  void SyncPlayerTargets()
  {
    if (deathVcam == null)
    {
      return;
    }

    PlayerDeathHandler player = FindFirstObjectByType<PlayerDeathHandler>();

    if (player == null)
    {
      return;
    }

    playerTransform = player.transform;
    EnsureOrbitPivot();
    UpdateOrbitPivotPosition();

    deathVcam.Follow = orbitPivot;
    deathVcam.LookAt = orbitPivot;
  }

  // Creates a world-space pivot used as the fixed-height orbit center.
  void EnsureOrbitPivot()
  {
    if (orbitPivot != null)
    {
      return;
    }

    GameObject pivotObject = new("DeathCamOrbitPivot");
    orbitPivot = pivotObject.transform;
  }

  // Keeps the pivot centered on the player XZ while locking to a fixed world-space Y.
  void UpdateOrbitPivotPosition()
  {
    if (orbitPivot == null || playerTransform == null)
    {
      return;
    }

    Vector3 playerPosition = playerTransform.position;
    orbitPivot.position = new Vector3(playerPosition.x, orbitPivotWorldY, playerPosition.z);
  }

  // Advances the death camera's horizontal orbit while the death view is active.
  void Update()
  {
    if (!isActive) return;

    UpdateOrbitPivotPosition();

    orbitalFollow.HorizontalAxis.Value = orbitalFollow.HorizontalAxis.ClampValue(
      orbitalFollow.HorizontalAxis.Value + (orbitSpeed * Time.deltaTime));
  }
}
