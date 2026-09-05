using UnityEngine;

// Shared contract for any object that can receive damage from combat systems.
public interface IDamageable
{
  bool TakeDamage(int amount, GameObject source, Vector3 hitPoint);
}
