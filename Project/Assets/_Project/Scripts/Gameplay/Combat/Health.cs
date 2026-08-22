using System;
using UnityEngine;

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

  void Awake()
  {
    CurrentHealth = maxHealth;
    blocker = GetComponent<IDamageBlocker>();
  }

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

  public void Heal(int amount)
  {
    if (amount <= 0 || IsDead) return;

    CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
    OnHealed?.Invoke(transform.position);
  }
}
