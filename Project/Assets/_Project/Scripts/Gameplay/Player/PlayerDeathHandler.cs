using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Animator))]
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

  void Awake()
  {
    health = GetComponent<Health>();
    animator = GetComponent<Animator>();
    locomotion = GetComponent<PlayerLocomotion>();
  }

  void OnEnable()
  {
    health.OnDied += HandleDied;
  }

  void OnDisable()
  {
    health.OnDied -= HandleDied;
  }

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

  IEnumerator RaiseGameOverAfterDelay()
  {
    yield return new WaitForSeconds(gameOverDelay);
    GameOverSignal.Raise(gameObject.scene.name);
  }

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
