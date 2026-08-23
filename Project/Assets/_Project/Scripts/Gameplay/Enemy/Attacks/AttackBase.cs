using System;
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

  public float MinRange => minRange;
  public float MaxRange => maxRange;
  public float MinAngle => minAngle;
  public float MaxAngle => maxAngle;
  public int Priority => priority;
  public float SelectionChance => selectionChance;
  public int Damage => damage;
  public virtual bool CanExecute => true;

  public event Action OnAttackComplete;

  public void Execute(EnemyController enemy)
  {
    OnExecute(enemy);
  }

  protected abstract void OnExecute(EnemyController enemy);

  public virtual void Interrupt() { }

  protected void PlayAttackAnimation(EnemyController enemy)
  {
    if (enemy.Animator == null)
    {
      return;
    }

    enemy.Animator.SetInteger(AttackIndexParam, attackAnimationIndex);
    enemy.Animator.SetTrigger(AttackParam);
  }

  protected void RaiseAttackComplete() => OnAttackComplete?.Invoke();
}
