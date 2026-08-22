using UnityEngine;

/// <summary>
/// Rolls a chance to spawn a health pickup when this enemy dies. Kept separate from EnemyAI since drop
/// config is per-enemy-type data, not AI behaviour - just a Health.OnDied subscriber.
/// </summary>
[RequireComponent(typeof(Health))]
public class EnemyLootDrop : MonoBehaviour
{
  [SerializeField] GameObject[] healthItemPrefabs;
  [SerializeField, Range(0f, 1f)] float dropChance = 0.25f;

  Health health;

  void Awake()
  {
    health = GetComponent<Health>();
  }

  void OnEnable()
  {
    health.OnDied += HandleDied;
  }

  void OnDisable()
  {
    health.OnDied -= HandleDied;
  }

  void HandleDied()
  {
    if (healthItemPrefabs == null || healthItemPrefabs.Length == 0 || Random.value > dropChance)
    {
      return;
    }

    GameObject prefab = healthItemPrefabs[Random.Range(0, healthItemPrefabs.Length)];
    Instantiate(prefab, transform.position, Quaternion.identity);
  }
}
