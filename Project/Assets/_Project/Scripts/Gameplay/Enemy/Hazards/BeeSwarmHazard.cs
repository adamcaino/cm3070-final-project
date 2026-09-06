using UnityEngine;

[RequireComponent(typeof(Collider))]
// Applies periodic damage to damageable objects inside the bee swarm trigger.
public class BeeSwarmHazard : MonoBehaviour, IAreaHazard
{
  [SerializeField, Min(0.05f)] float tickInterval = 1f;

  int damage;
  float nextTickTime;

  // Configures the attached collider as a trigger.
  void Awake()
  {
    GetComponent<Collider>().isTrigger = true;
  }

  // Stores the damage value supplied by the hazard spawner.
  public void Configure(int hazardDamage)
  {
    damage = hazardDamage;
  }

  // Attempts to damage a target when it enters the hazard.
  void OnTriggerEnter(Collider other) => TryTick(other);

  // Attempts subsequent damage ticks while a target remains inside the hazard.
  void OnTriggerStay(Collider other) => TryTick(other);

  // Applies damage when the shared hazard tick interval has elapsed.
  void TryTick(Collider other)
  {
    if (Time.time < nextTickTime) return;

    IDamageable damageable = other.GetComponentInParent<IDamageable>();
    if (damageable == null) return;

    nextTickTime = Time.time + tickInterval;
    damageable.TakeDamage(damage, gameObject, other.ClosestPoint(transform.position));
  }
}
