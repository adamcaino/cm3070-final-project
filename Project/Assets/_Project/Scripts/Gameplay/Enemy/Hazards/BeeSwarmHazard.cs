using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BeeSwarmHazard : MonoBehaviour, IAreaHazard
{
  [SerializeField, Min(0.05f)] float tickInterval = 1f;

  int damage;
  float nextTickTime;

  void Awake()
  {
    GetComponent<Collider>().isTrigger = true;
  }

  public void Configure(int hazardDamage)
  {
    damage = hazardDamage;
  }

  void OnTriggerEnter(Collider other) => TryTick(other);
  void OnTriggerStay(Collider other) => TryTick(other);

  void TryTick(Collider other)
  {
    if (Time.time < nextTickTime) return;

    IDamageable damageable = other.GetComponentInParent<IDamageable>();
    if (damageable == null) return;

    nextTickTime = Time.time + tickInterval;
    damageable.TakeDamage(damage, gameObject, other.ClosestPoint(transform.position));
  }
}
