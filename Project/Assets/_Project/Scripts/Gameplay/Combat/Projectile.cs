using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
  [SerializeField, Min(0f)] float lifetime = 5f;

  Vector3 direction;
  float speed;
  int damage;
  GameObject source;

  void Awake()
  {
    GetComponent<Collider>().isTrigger = true;
  }

  // Stores the projectile's state so it can move and resolve impact without needing a persistent owner.
  public void Launch(Vector3 launchDirection, float launchSpeed, int launchDamage, GameObject launchSource)
  {
    direction = launchDirection.normalized;
    speed = launchSpeed;
    damage = launchDamage;
    source = launchSource;

    if (direction.sqrMagnitude > 0.0001f)
    {
      transform.rotation = Quaternion.LookRotation(direction);
    }

    Destroy(gameObject, lifetime);
  }

  void Update()
  {
    transform.position += direction * speed * Time.deltaTime;
  }

  void OnTriggerEnter(Collider other)
  {
    if (source != null && other.gameObject == source)
    {
      return;
    }

    IDamageable damageable = other.GetComponent<IDamageable>();
    if (damageable == null)
    {
      return;
    }

    damageable.TakeDamage(damage, source, other.ClosestPoint(transform.position));
    Destroy(gameObject);
  }
}
