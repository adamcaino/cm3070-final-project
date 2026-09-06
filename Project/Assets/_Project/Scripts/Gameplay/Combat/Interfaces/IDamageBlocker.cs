using UnityEngine;

// Defines the operation required by components that can intercept incoming damage.
public interface IDamageBlocker
{
  // Tests whether the incoming hit from the source at the contact point is blocked.
  bool TryBlock(GameObject source, Vector3 hitPoint);
}
