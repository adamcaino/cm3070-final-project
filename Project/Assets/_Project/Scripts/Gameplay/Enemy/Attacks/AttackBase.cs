using System;
using System.Collections;
using UnityEngine;

public abstract class AttackBase : MonoBehaviour, IAttack
{
  static readonly int AttackParam = Animator.StringToHash("attack");
  static readonly int AttackIndexParam = Animator.StringToHash("attackIndex");

  [Header("Attack")]
  [SerializeField, Min(0f)] float minRange;
  [SerializeField, Min(0f)] float maxRange = 5f;
  [SerializeField, Range(0f, 180f)] float minAngle;
  [SerializeField, Range(0f, 180f)] float maxAngle = 180f;
  [SerializeField] int priority;
  [Tooltip("Odds this attack is taken when it's the highest-priority match in range. 1 always wins, matching plain priority ordering; lower values let it be skipped in favour of the next-best match.")]
  [SerializeField, Range(0f, 1f)] float selectionChance = 1f;
  [SerializeField] int attackAnimationIndex;
  [SerializeField, Min(0)] int damage;
  [Tooltip("Fallback completion time for attacks that rely on an Animation Event to call RaiseAttackComplete. If the event is missing or never fires, this forces completion so the enemy state machine can't stall forever. Set to 0 to disable.")]
  [SerializeField, Min(0f)] float completionTimeout = 3f;

  [Header("Boss Stage Gating")]
  [Tooltip("Minimum boss phase stage required before this attack becomes available. Ignored on enemies with no BossPhaseController.")]
  [SerializeField, Min(1)] int unlockStage = 1;

  bool completedThisExecution;
  Coroutine completionTimeoutRoutine;
  BossPhaseController phase;

  public float MinRange => minRange;
  public float MaxRange => maxRange;
  public float MinAngle => minAngle;
  public float MaxAngle => maxAngle;
  public int Priority => priority;
  public virtual float SelectionChance => selectionChance;
  public int Damage => damage;
  public virtual bool CanExecute => phase == null || phase.CurrentStage >= unlockStage;

  public event Action OnAttackComplete;

  // True from Execute until this attack completes. Animation Events broadcast by function name to every
  // matching method on the GameObject, so sibling attacks sharing a hitbox must check this before acting
  // on an event meant for whichever attack is actually playing.
  protected bool IsExecuting { get; private set; }

  protected virtual void Awake()
  {
    phase = GetComponent<BossPhaseController>();
  }

  public void Execute(EnemyController enemy)
  {
    completedThisExecution = false;
    IsExecuting = true;
    OnExecute(enemy);

    if (!completedThisExecution && completionTimeout > 0f)
    {
      completionTimeoutRoutine = StartCoroutine(CompletionTimeoutRoutine());
    }
  }

  protected abstract void OnExecute(EnemyController enemy);

  public virtual void Interrupt() { }

  IEnumerator CompletionTimeoutRoutine()
  {
    yield return new WaitForSeconds(completionTimeout);
    RaiseAttackComplete();
  }

  protected void PlayAttackAnimation(EnemyController enemy)
  {
    if (enemy.Animator == null)
    {
      return;
    }

    enemy.Animator.SetInteger(AttackIndexParam, attackAnimationIndex);
    enemy.Animator.SetTrigger(AttackParam);
  }

  protected void RaiseAttackComplete()
  {
    if (completedThisExecution) return;
    completedThisExecution = true;
    IsExecuting = false;

    if (completionTimeoutRoutine != null)
    {
      StopCoroutine(completionTimeoutRoutine);
      completionTimeoutRoutine = null;
    }

    OnAttackComplete?.Invoke();
  }
}
