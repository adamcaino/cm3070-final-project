using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class PlayerAttack : MonoBehaviour
{
  static readonly int AttackParam = Animator.StringToHash("attack");
  static readonly int DefendParam = Animator.StringToHash("isDefending");

  [Header("Input")]
  [SerializeField] InputActionReference attackAction;
  [SerializeField] InputActionReference defendAction;

  Animator animator;
  bool isDefending;

  public bool IsDefending => isDefending;

  void Awake()
  {
    animator = GetComponent<Animator>();
  }

  void OnEnable()
  {
    if (attackAction == null || defendAction == null) return;

    attackAction.action.performed += OnAttackPerformed;
    attackAction.action.Enable();

    defendAction.action.performed += OnDefendChanged;
    defendAction.action.canceled += OnDefendChanged;
    defendAction.action.Enable();
  }

  void OnDisable()
  {
    if (attackAction == null || defendAction == null) return;

    attackAction.action.performed -= OnAttackPerformed;
    attackAction.action.Disable();

    defendAction.action.performed -= OnDefendChanged;
    defendAction.action.canceled -= OnDefendChanged;
    defendAction.action.Disable();
  }

  void OnAttackPerformed(InputAction.CallbackContext context)
  {
    if (isDefending) return;

    animator.SetTrigger(AttackParam);
  }

  void OnDefendChanged(InputAction.CallbackContext context)
  {
    isDefending = context.performed;
    animator.SetBool(DefendParam, isDefending);
  }
}
