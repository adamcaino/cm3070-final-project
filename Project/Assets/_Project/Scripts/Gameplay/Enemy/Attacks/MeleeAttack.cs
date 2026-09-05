using System.Collections.Generic;
using UnityEngine;

public class MeleeAttack : MeleeHitboxAttack
{
  readonly HashSet<IDamageable> hitThisSwing = new HashSet<IDamageable>();
  bool isHitboxActive;

  protected override void OnExecute(EnemyController enemy)
  {
    PlayAttackAnimation(enemy);
  }

  void OnTriggerEnter(Collider other)
  {
    if (!isHitboxActive) return;

    IDamageable damageable = other.GetComponentInParent<IDamageable>();
    if (damageable == null || !hitThisSwing.Add(damageable)) return;

    DealDamage(damageable, other);
  }

  public void EnableHitbox()
  {
    if (!IsExecuting) return;

    hitThisSwing.Clear();
    isHitboxActive = true;

    ActivateHitbox();
  }

  public void DisableHitbox()
  {
    if (!isHitboxActive) return;

    isHitboxActive = false;

    DeactivateHitbox();
  }
}
