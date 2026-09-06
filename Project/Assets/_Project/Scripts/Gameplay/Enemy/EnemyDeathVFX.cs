using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Animator))]
// Spawns the appropriate death effect after the enemy's death animation timing.
public class EnemyDeathVFX : MonoBehaviour
{
  [SerializeField] GameObject deathVfxPrefab;
  [SerializeField] GameObject frozenDeathVfxPrefab;
  [SerializeField, Min(0f)] float vfxLifetime = 3.5f;
  [Tooltip("Safety cap while waiting for the Animator to reach the Die state, in case it's missing or misnamed on this controller - without it, a broken setup would stall cleanup forever.")]
  [SerializeField, Min(0.1f)] float dieStateDetectTimeout = 1f;

  Health health;
  StatusEffectReceiver statusEffects;
  Renderer meshRenderer;
  Animator animator;

  // Caches the health, status-effect, animator, and mesh components.
  void Awake()
  {
    health = GetComponent<Health>();
    statusEffects = GetComponent<StatusEffectReceiver>();
    animator = GetComponent<Animator>();

    meshRenderer = GetComponentInChildren<Renderer>();
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

  // Starts the death effect routine and records whether the enemy died frozen.
  void HandleDied()
  {
    bool diedFrozen = statusEffects != null && statusEffects.IsFrozen;
    StartCoroutine(DeathRoutine(diedFrozen));
  }

  // Waits for the death animation, spawns the effect, and destroys the enemy object.
  IEnumerator DeathRoutine(bool diedFrozen)
  {
    if (!diedFrozen)
    {
      yield return EnemyDeathAnimationWait.ForDieState(animator, dieStateDetectTimeout);
    }
    else
    {
      yield return null;
    }

    GameObject vfxPrefab = diedFrozen ? frozenDeathVfxPrefab : deathVfxPrefab;
    if (vfxPrefab != null)
    {
      Vector3 spawnPosition = meshRenderer != null ? meshRenderer.bounds.center : transform.position;
      GameObject vfx = Instantiate(vfxPrefab, spawnPosition, transform.rotation);
      Destroy(vfx, vfxLifetime);
    }

    Destroy(gameObject);
  }
}
