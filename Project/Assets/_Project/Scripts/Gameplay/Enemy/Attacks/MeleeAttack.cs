using System;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttack : AttackBase
{
  [SerializeField] Collider hitCollider;
  EnemyController owner => GetComponent<EnemyController>();

  public event Action<Collider> OnHit;

  readonly HashSet<IDamageable> hitThisArm = new HashSet<IDamageable>();
  bool isHitboxActive;

  void OnEnable()
  {
    if (hitCollider != null)
    {
      OnHit += HandleHit;
    }
  }

  void OnDisable()
  {
    if (hitCollider != null)
    {
      OnHit -= HandleHit;
    }
  }

  void Awake()
  {
    if (hitCollider == null)
    {
      Debug.LogWarning($"{name}: MeleeAttack has no Hit Collider assigned.", this);
      return;
    }

    hitCollider.enabled = false;
  }

  protected override void OnExecute(EnemyController enemy)
  {
    PlayAttackAnimation(enemy);
  }

  void HandleHit(Collider other)
  {
    IDamageable damageable = other.GetComponentInParent<IDamageable>();

    Vector3 hitPoint = other.ClosestPointOnBounds(hitCollider.transform.position);
    damageable?.TakeDamage(GetComponent<AttackBase>().Damage, owner != null ? owner.gameObject : gameObject, hitPoint);
  }

  void OnTriggerEnter(Collider other)
  {
    if (!isHitboxActive) return;

    IDamageable damageable = other.GetComponentInParent<IDamageable>();
    if (damageable == null) return;

    if (!hitThisArm.Add(damageable)) return;

    OnHit?.Invoke(other);
  }

  public void EnableHitbox()
  {
    hitThisArm.Clear();

    hitCollider.enabled = true;
    isHitboxActive = true;
  }

  public void DisableHitbox()
  {
    isHitboxActive = false;
    hitCollider.enabled = false;

    RaiseAttackComplete();
  }
}
