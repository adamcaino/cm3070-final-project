using UnityEngine;

// Shared hitbox-collider plumbing for attacks that damage IDamageables touching a collider while it's
// active - subclasses decide how a touch translates into damage (single hit vs. damage over time) since
// that differs per attack, but share the collider enable/disable and the actual TakeDamage call.
// Subclasses expose their own activate/deactivate entry points under distinct names because Animator
// animation events call a method by name across every component on the GameObject; two attacks sharing
// the same method names would fire together.
public abstract class MeleeHitboxAttack : AttackBase
{
  [SerializeField] Collider hitCollider;
  EnemyController owner => GetComponent<EnemyController>();

  void Awake()
  {
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
