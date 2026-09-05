using UnityEngine;

// Optional defensive behaviour for actors that can absorb or redirect incoming damage.
public interface IDamageBlocker
{
  bool TryBlock(GameObject source, Vector3 hitPoint);
}
