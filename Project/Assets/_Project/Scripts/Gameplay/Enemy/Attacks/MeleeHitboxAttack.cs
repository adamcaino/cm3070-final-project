using UnityEngine;

// Provides shared collider setup and damage application for melee attacks.
public abstract class MeleeHitboxAttack : AttackBase
{
  [SerializeField] Collider hitCollider;
  EnemyController owner => GetComponent<EnemyController>();

  // Validates and disables the configured hit collider during initialization.
  protected override void Awake()
  {
    base.Awake();

    if (hitCollider == null)
    {
      Debug.LogWarning($"{name}: {GetType().Name} has no Hit Collider assigned.", this);
      return;
    }

    hitCollider.enabled = false;
  }

  // Applies attack damage at the closest point between the target and hit collider.
  protected void DealDamage(IDamageable damageable, Collider other)
  {
    Vector3 hitPoint = other.ClosestPointOnBounds(hitCollider.transform.position);
    damageable.TakeDamage(Damage, owner != null ? owner.gameObject : gameObject, hitPoint);
  }

  // Enables the collider used by the melee attack.
  protected void ActivateHitbox()
  {
    hitCollider.enabled = true;
  }

  // Disables the collider and signals attack completion.
  protected void DeactivateHitbox()
  {
    hitCollider.enabled = false;
    RaiseAttackComplete();
  }
}
