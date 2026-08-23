using UnityEngine;

public interface IAfflictable
{
  void ApplyAffliction(AfflictionType type, float duration, int magnitude, GameObject source);
}
