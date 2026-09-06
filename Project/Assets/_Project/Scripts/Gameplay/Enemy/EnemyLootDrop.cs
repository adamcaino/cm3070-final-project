using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Animator))]
// Rolls for a health pickup when the enemy dies and delays its reveal until the death animation.
public class EnemyLootDrop : MonoBehaviour
{
  [SerializeField] GameObject healthItemPrefab;
  [SerializeField, Range(0f, 1f)] float dropChance = 0.1f;
  [Tooltip("Fallback reveal value while waiting for the Animator to reach the Die state.")]
  [SerializeField, Min(0.1f)] float dieStateDetectTimeout = 1f;

  Health health;
  StatusEffectReceiver statusEffects;
  Animator animator;

  // Caches the health, status-effect, and animator components used by the drop flow.
  void Awake()
  {
    health = GetComponent<Health>();
    statusEffects = GetComponent<StatusEffectReceiver>();
    animator = GetComponent<Animator>();
  }

  // Subscribes to the enemy death event.
  void OnEnable()
  {
    health.OnDied += HandleDied;
  }

  // Removes the enemy death event subscription.
  void OnDisable()
  {
    health.OnDied -= HandleDied;
  }

  // Performs the drop roll and creates a delayed-reveal pickup when successful.
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
