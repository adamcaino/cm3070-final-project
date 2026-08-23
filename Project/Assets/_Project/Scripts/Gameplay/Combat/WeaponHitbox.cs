using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class WeaponHitbox : MonoBehaviour
{
  Collider hitCollider;

  // Keyed by IDamageable so multiple colliders count as one hit per swing.
  readonly HashSet<IDamageable> hitThisSwing = new HashSet<IDamageable>();

  public event Action<Collider> OnHit;

  void Awake()
  {
    hitCollider = GetComponent<Collider>();
    hitCollider.isTrigger = true;
    hitCollider.enabled = false;

    Rigidbody rb = GetComponent<Rigidbody>();
    rb.isKinematic = true;
    rb.useGravity = false;
  }

  public void EnableHitbox()
  {
    hitThisSwing.Clear();
    hitCollider.enabled = true;
  }

  public void DisableHitbox()
  {
    hitCollider.enabled = false;
  }

  void OnTriggerEnter(Collider other)
  {
    // Prevent Player from hitting themselves.
    if (other.CompareTag("Player")) return;

    // Find damageable components on parent objects of child colliders.
    IDamageable damageable = other.GetComponentInParent<IDamageable>();

    if (damageable == null) return;
    if (!hitThisSwing.Add(damageable)) return;

    OnHit?.Invoke(other);
  }
}
