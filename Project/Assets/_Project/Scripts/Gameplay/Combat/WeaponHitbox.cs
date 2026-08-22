using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class WeaponHitbox : MonoBehaviour
{
  Collider hitCollider;

  // Keyed by IDamageable rather than Collider - a target can present more than one collider, and those
  // should still only ever count as a single hit per swing.
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

    // GetComponentInParent, not GetComponent - the collider that's actually hit (e.g. an enemy's Body
    // collider on a child bone) often isn't on the same GameObject as Health itself.
    IDamageable damageable = other.GetComponentInParent<IDamageable>();
    if (damageable == null) return;
    if (!hitThisSwing.Add(damageable)) return;

    OnHit?.Invoke(other);
  }
}
