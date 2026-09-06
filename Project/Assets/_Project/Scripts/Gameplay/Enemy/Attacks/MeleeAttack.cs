using System.Collections.Generic;
using UnityEngine;

// Applies one damage event per target while its melee hitbox is active.
public class MeleeAttack : MeleeHitboxAttack
{
  readonly HashSet<IDamageable> hitThisSwing = new HashSet<IDamageable>();
  bool isHitboxActive;

  // Starts the configured melee animation.
  protected override void OnExecute(EnemyController enemy)
  {
    PlayAttackAnimation(enemy);
  }

  // Damages each target once during the active hitbox window.
  void OnTriggerEnter(Collider other)
  {
    if (!isHitboxActive) return;

    IDamageable damageable = other.GetComponentInParent<IDamageable>();
    if (damageable == null || !hitThisSwing.Add(damageable)) return;

    DealDamage(damageable, other);
  }

  // Clears previous targets and activates the melee hitbox during execution.
  public void EnableHitbox()
  {
    if (!IsExecuting) return;

    hitThisSwing.Clear();
    isHitboxActive = true;

    ActivateHitbox();
  }

  // Deactivates the hitbox and allows the attack to complete.
  public void DisableHitbox()
  {
    if (!isHitboxActive) return;

    isHitboxActive = false;

    DeactivateHitbox();
  }
}
