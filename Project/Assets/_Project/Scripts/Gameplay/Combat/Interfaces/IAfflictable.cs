using UnityEngine;

// Defines the operation required by actors that can receive combat status effects.
public interface IAfflictable
{
  // Applies a status effect with the supplied duration, strength, and source actor.
  void ApplyAffliction(AfflictionType type, float duration, int magnitude, GameObject source);
}
