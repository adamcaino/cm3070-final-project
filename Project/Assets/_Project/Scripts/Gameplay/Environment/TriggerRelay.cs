using UnityEngine;






[RequireComponent(typeof(Collider))]
// Forwards player trigger entries to an ITriggerable component on a configured target.
public class TriggerRelay : MonoBehaviour
{
  [SerializeField] GameObject target;

  ITriggerable triggerable;

  const string PLAYERTAG = "Player";

  // Caches the target's triggerable component when the relay is initialized.
  void Awake()
  {
    if (target != null)
    {
      triggerable = target.GetComponent<ITriggerable>();
    }
  }



  // Assigns a target object and refreshes its triggerable component reference.
  public void Configure(GameObject targetObject)
  {
    target = targetObject;
    triggerable = target != null ? target.GetComponent<ITriggerable>() : null;
  }

  // Forwards the player's world position when the relay receives a trigger entry.
  void OnTriggerEnter(Collider other)
  {
    if (triggerable == null || !other.CompareTag(PLAYERTAG))
    {
      return;
    }


    triggerable.OnTriggered(other.transform.position);
  }
}
