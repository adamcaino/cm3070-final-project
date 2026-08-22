using UnityEngine;

/// <summary>
/// Anything that can be burned/frozen/etc. Mirrors IDamageable's shape - a weapon's OnHit handler
/// checks for both on the same collider and applies whichever the target actually supports.
/// </summary>
public interface IAfflictable
{
  void ApplyAffliction(AfflictionType type, float duration, int magnitude, GameObject source);
}
