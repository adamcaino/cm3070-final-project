using UnityEngine;

public abstract class MeleeHitboxAttack : AttackBase
{
  [SerializeField] Collider hitCollider;
  EnemyController owner => GetComponent<EnemyController>();

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

  protected void DealDamage(IDamageable damageable, Collider other)
  {
    Vector3 hitPoint = other.ClosestPointOnBounds(hitCollider.transform.position);
    damageable.TakeDamage(Damage, owner != null ? owner.gameObject : gameObject, hitPoint);
  }

  protected void ActivateHitbox()
  {
    hitCollider.enabled = true;
  }

  protected void DeactivateHitbox()
  {
    hitCollider.enabled = false;
    RaiseAttackComplete();
  }
}
