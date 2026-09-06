using System;
using UnityEngine;

// Tracks HP, applies incoming damage, and raises events for damage, healing, and death.
public class Health : MonoBehaviour, IDamageable
{
  [SerializeField, Min(1)] int maxHealth = 3;

  public int MaxHealth => maxHealth;
  public int CurrentHealth { get; private set; }
  public bool IsDead => CurrentHealth <= 0;

  public event Action<Vector3> OnDamaged;
  public event Action<Vector3> OnHealed;
  public event Action OnDied;

  IDamageBlocker blocker;

  // Initializes the actor at maximum health and caches an optional damage blocker.
  void Awake()
  {
    CurrentHealth = maxHealth;
    blocker = GetComponent<IDamageBlocker>();
  }

  // Returns true only when the hit actually kills this actor.
  public bool TakeDamage(int amount, GameObject source, Vector3 hitPoint)
  {
    if (IsDead || amount <= 0) return false;

    if (blocker != null && blocker.TryBlock(source, hitPoint))
    {
      return false;
    }

    CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
    OnDamaged?.Invoke(hitPoint);

    if (!IsDead)
    {
      return false;
    }

    OnDied?.Invoke();
    return true;
  }

  // Clamps healing so the actor cannot exceed max HP or be revived after death.
  public void Heal(int amount)
  {
    if (amount <= 0 || IsDead) return;

    CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
    OnHealed?.Invoke(transform.position);
  }
}
