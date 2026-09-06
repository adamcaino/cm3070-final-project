using UnityEngine;

// Defines the operation required by objects that can receive combat damage.
public interface IDamageable
{
  // Applies damage and reports whether the hit reduced the target to zero health.
  bool TakeDamage(int amount, GameObject source, Vector3 hitPoint);
}
