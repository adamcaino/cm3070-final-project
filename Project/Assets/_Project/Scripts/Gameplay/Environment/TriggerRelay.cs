using UnityEngine;






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



  public void Configure(GameObject targetObject)
  {
    target = targetObject;
    triggerable = target != null ? target.GetComponent<ITriggerable>() : null;
  }

  void OnTriggerEnter(Collider other)
  {
    if (triggerable == null || !other.CompareTag(PLAYERTAG))
    {
      return;
    }


    triggerable.OnTriggered(other.transform.position);
  }
}
