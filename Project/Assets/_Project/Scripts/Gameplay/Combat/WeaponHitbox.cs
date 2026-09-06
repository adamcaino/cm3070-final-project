using System;
using System.Collections.Generic;
using UnityEngine;

// Enables a hit window for melee attacks and prevents repeated hits from the same swing.
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class WeaponHitbox : MonoBehaviour
{
  Collider hitCollider;

  readonly HashSet<IDamageable> hitThisSwing = new HashSet<IDamageable>();

  public event Action<Collider> OnHit;

  // Configures the collider as a disabled kinematic trigger for hit detection.
  void Awake()
  {
    hitCollider = GetComponent<Collider>();
    hitCollider.isTrigger = true;
    hitCollider.enabled = false;

    Rigidbody rb = GetComponent<Rigidbody>();
    rb.isKinematic = true;
    rb.useGravity = false;
  }

  // Clears the previous swing's targets and enables hit detection.
  public void EnableHitbox()
  {
    hitThisSwing.Clear();
    hitCollider.enabled = true;
  }

  // Disables hit detection for the current swing.
  public void DisableHitbox()
  {
    hitCollider.enabled = false;
  }

  // Reports the first hit on each damageable target during the active swing.
  void OnTriggerEnter(Collider other)
  {
    if (other.CompareTag("Player")) return;

    IDamageable damageable = other.GetComponentInParent<IDamageable>();

    if (damageable == null) return;
    if (!hitThisSwing.Add(damageable)) return;

    OnHit?.Invoke(other);
  }
}
