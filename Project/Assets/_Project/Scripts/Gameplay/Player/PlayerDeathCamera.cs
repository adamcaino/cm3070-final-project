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

  CinemachineCamera deathVcam;
  CinemachineOrbitalFollow orbitalFollow;
  bool isActive;

  void Awake()
  {
    deathVcam = GetComponent<CinemachineCamera>();
    orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
    RefreshRuntimeReferences();
  }

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

  void OnEnable()
  {
    PlayerDiedSignal.Raised += HandlePlayerDied;
  }

  void OnDisable()
  {
    PlayerDiedSignal.Raised -= HandlePlayerDied;
  }

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

    deathVcam.Follow = player.transform;
    deathVcam.LookAt = player.transform;
  }

  void Update()
  {
    if (!isActive) return;

    orbitalFollow.HorizontalAxis.Value = orbitalFollow.HorizontalAxis.ClampValue(
      orbitalFollow.HorizontalAxis.Value + (orbitSpeed * Time.deltaTime));
  }
}
