using UnityEngine;

// Implemented by actors that can be affected by status effects like burn or freeze.
public interface IAfflictable
{
  void ApplyAffliction(AfflictionType type, float duration, int magnitude, GameObject source);
}
