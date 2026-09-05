using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Animator))]
public class EnemyLootDrop : MonoBehaviour
{
  [SerializeField] GameObject healthItemPrefab;
  [SerializeField, Range(0f, 1f)] float dropChance = 0.1f;
  [Tooltip("Fallback reveal value while waiting for the Animator to reach the Die state.")]
  [SerializeField, Min(0.1f)] float dieStateDetectTimeout = 1f;

  Health health;
  StatusEffectReceiver statusEffects;
  Animator animator;

  void Awake()
  {
    health = GetComponent<Health>();
    statusEffects = GetComponent<StatusEffectReceiver>();
    animator = GetComponent<Animator>();
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
    if (healthItemPrefab == null || Random.value > dropChance)
    {
      return;
    }

    bool diedFrozen = statusEffects != null && statusEffects.IsFrozen;

    GameObject drop = Instantiate(healthItemPrefab, transform.position, Quaternion.identity);
    drop.AddComponent<DelayedPickupReveal>().RevealAfterDieAnimation(animator, diedFrozen, dieStateDetectTimeout);
  }
}
