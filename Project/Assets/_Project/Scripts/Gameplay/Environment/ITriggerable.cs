using UnityEngine;

// Defines the callback used by objects that respond to a trigger source.
public interface ITriggerable
{
  // Handles activation from the supplied world position.
  void OnTriggered(Vector3 sourcePosition);
}
