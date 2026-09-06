using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;







// Adjusts the gameplay camera distance from zoom input until player death or victory.
public class PlayerCameraZoom : MonoBehaviour
{
  [Header("Input")]
  [SerializeField] InputActionReference zoomAction;

  [Header("Target")]
  [SerializeField] CinemachineOrbitalFollow orbitalFollow;

  [Header("Zoom Range")]
  [SerializeField, Min(0.1f)] float minDistance = 3f;
  [SerializeField, Min(0.1f)] float maxDistance = 12f;
  [SerializeField, Min(0f)] float zoomSpeed = 5f;

  float targetDistance;
  bool isDead;

  // Resolves the gameplay orbital follow and stores its initial radius.
  void Awake()
  {
    if (orbitalFollow == null)
    {
      orbitalFollow = PlayerCameraOrbit.ResolveGameplayOrbitalFollow();
    }

    if (orbitalFollow != null)
    {
      targetDistance = orbitalFollow.Radius;
    }
  }

  // Enables zoom input and subscribes to death and victory signals.
  void OnEnable()
  {
    zoomAction?.action.Enable();
    PlayerDiedSignal.Raised += HandlePlayerDied;
    GameOverSignal.VictoryRaised += HandleVictoryRaised;
  }

  // Disables zoom input and removes death and victory subscriptions.
  void OnDisable()
  {
    zoomAction?.action.Disable();
    PlayerDiedSignal.Raised -= HandlePlayerDied;
    GameOverSignal.VictoryRaised -= HandleVictoryRaised;
  }

  // Reads zoom input, clamps the target distance, and smoothly updates the camera radius.
  void Update()
  {
    if (isDead || orbitalFollow == null || zoomAction == null)
    {
      return;
    }

    float zoomInput = zoomAction.action.ReadValue<float>();
    if (Mathf.Abs(zoomInput) > 0.0001f)
    {
      targetDistance = Mathf.Clamp(targetDistance - zoomInput, minDistance, maxDistance);
    }

    orbitalFollow.Radius = Mathf.Lerp(orbitalFollow.Radius, targetDistance, zoomSpeed * Time.deltaTime);
  }

  // Stops zoom input after the player dies.
  void HandlePlayerDied()
  {
    isDead = true;
    zoomAction?.action.Disable();
  }

  // Stops zoom input when victory belongs to this scene.
  void HandleVictoryRaised(string sceneName)
  {
    if (sceneName != gameObject.scene.name)
    {
      return;
    }

    isDead = true;
    zoomAction?.action.Disable();
  }
}
