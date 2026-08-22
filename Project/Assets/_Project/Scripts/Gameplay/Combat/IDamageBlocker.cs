using UnityEngine;

/// <summary>
/// Optional sibling component that gets first refusal on incoming damage - lets things like a
/// directional shield block reduce an attack to 0 without Health needing to know about blocking rules.
/// </summary>
public interface IDamageBlocker
{
  bool TryBlock(GameObject source, Vector3 hitPoint);
}
