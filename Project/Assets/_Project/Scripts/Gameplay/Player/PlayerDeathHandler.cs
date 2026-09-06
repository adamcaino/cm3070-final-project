using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Animator))]
// Handles player death animation, input shutdown, immediate death notification, and game over delay.
public class PlayerDeathHandler : MonoBehaviour
{
  static readonly int DeathTrigger = Animator.StringToHash("death");

  [SerializeField, Min(0f)] float gameOverDelay = 3f;

  [Header("Input")]
  [SerializeField] InputActionAsset playerControls;
  [SerializeField] string playerActionMapName = "Player";

  Health health;
  Animator animator;
  PlayerLocomotion locomotion;

  // Caches health, animation, and locomotion components.
  void Awake()
  {
    health = GetComponent<Health>();
    animator = GetComponent<Animator>();
    locomotion = GetComponent<PlayerLocomotion>();
  }

  // Subscribes to the player's death event.
  void OnEnable()
  {
    health.OnDied += HandleDied;
  }

  // Removes the player's death event subscription.
  void OnDisable()
  {
    health.OnDied -= HandleDied;
  }

  // Starts the death sequence, disables movement and controls, and schedules game over.
  void HandleDied()
  {
    animator.SetTrigger(DeathTrigger);
    PlayerDiedSignal.Raise();

    if (locomotion != null)
    {
      locomotion.enabled = false;
    }

    SetPlayerControlsEnabled(false);
    StartCoroutine(RaiseGameOverAfterDelay());
  }

  // Waits for the configured delay before raising the game-over signal.
  IEnumerator RaiseGameOverAfterDelay()
  {
    yield return new WaitForSeconds(gameOverDelay);
    GameOverSignal.Raise(gameObject.scene.name);
  }

  // Enables or disables the configured player input action map.
  void SetPlayerControlsEnabled(bool isEnabled)
  {
    InputActionMap map = playerControls != null ? playerControls.FindActionMap(playerActionMapName) : null;
    if (map == null) return;

    if (isEnabled)
    {
      map.Enable();
    }
    else
    {
      map.Disable();
    }
  }
}
