using UnityEngine;

public interface IDamageable
{
  bool TakeDamage(int amount, GameObject source, Vector3 hitPoint);
}
