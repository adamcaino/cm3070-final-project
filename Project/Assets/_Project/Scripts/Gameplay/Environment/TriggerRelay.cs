using UnityEngine;

/// <summary>
/// Sits on a standalone trigger collider GameObject and forwards player contact to a target's
/// ITriggerable, decoupling collider placement (which may not want to inherit a rotating
/// parent's transform) from the reaction logic.
/// </summary>
[RequireComponent(typeof(Collider))]
public class TriggerRelay : MonoBehaviour
{
  [SerializeField] GameObject target;

  ITriggerable triggerable;

  const string PLAYERTAG = "Player";

  void Awake()
  {
    if (target != null)
    {
      triggerable = target.GetComponent<ITriggerable>();
    }
  }

  void OnTriggerEnter(Collider other)
  {
    if (triggerable == null || !other.CompareTag(PLAYERTAG))
    {
      return;
    }

    // Pass the player's position to the target's OnTriggered method, so it can determine which side of the trigger the player is on.
    triggerable.OnTriggered(other.transform.position);
  }
}
