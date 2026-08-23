using UnityEngine;

// Optional sibling component that gets first refusal on incoming damage - lets a directional 
// shield block reduce an attack to 0 without Health needing to know about blocking rules.
public interface IDamageBlocker
{
  bool TryBlock(GameObject source, Vector3 hitPoint);
}
