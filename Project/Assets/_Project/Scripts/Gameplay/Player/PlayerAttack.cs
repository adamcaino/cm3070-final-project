using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
// Handles player attack and defend input and updates the corresponding animator parameters.
public class PlayerAttack : MonoBehaviour
{
  static readonly int AttackParam = Animator.StringToHash("attack");
  static readonly int DefendParam = Animator.StringToHash("isDefending");

  [Header("Input")]
  [SerializeField] InputActionReference attackAction;
  [SerializeField] InputActionReference defendAction;

  Animator animator;
  bool isDefending;
  bool isInputLocked;

  public bool IsDefending => isDefending;

  // Caches the animator used by attack and defence actions.
  void Awake()
  {
    animator = GetComponent<Animator>();
  }

  // Subscribes to victory and input action events when the component is enabled.
  void OnEnable()
  {
    GameOverSignal.VictoryRaised += HandleVictoryRaised;

    if (attackAction == null || defendAction == null) return;

    attackAction.action.performed += OnAttackPerformed;
    attackAction.action.Enable();

    defendAction.action.performed += OnDefendChanged;
    defendAction.action.canceled += OnDefendChanged;
    defendAction.action.Enable();
  }

  // Removes victory and input action subscriptions when the component is disabled.
  void OnDisable()
  {
    GameOverSignal.VictoryRaised -= HandleVictoryRaised;

    if (attackAction == null || defendAction == null) return;

    attackAction.action.performed -= OnAttackPerformed;
    attackAction.action.Disable();

    defendAction.action.performed -= OnDefendChanged;
    defendAction.action.canceled -= OnDefendChanged;
    defendAction.action.Disable();
  }

  // Triggers an attack unless input is locked or the player is defending.
  void OnAttackPerformed(InputAction.CallbackContext context)
  {
    if (isInputLocked || isDefending) return;

    animator.SetTrigger(AttackParam);
  }

  // Updates the defending state from the performed or cancelled input action.
  void OnDefendChanged(InputAction.CallbackContext context)
  {
    isDefending = context.performed;
    animator.SetBool(DefendParam, isDefending);
  }

  // Locks combat input when victory belongs to this scene.
  void HandleVictoryRaised(string sceneName)
  {
    if (sceneName != gameObject.scene.name)
    {
      return;
    }

    isInputLocked = true;
    isDefending = false;
    animator.SetBool(DefendParam, false);
    attackAction?.action.Disable();
    defendAction?.action.Disable();
  }
}
