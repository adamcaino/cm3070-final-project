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
    if (other.CompareTag("Player")) return;

    IDamageable damageable = other.GetComponentInParent<IDamageable>();

    if (damageable == null) return;
    if (!hitThisSwing.Add(damageable)) return;

    OnHit?.Invoke(other);
  }
}
