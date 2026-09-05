using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Animator))]
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

  void Awake()
  {
    health = GetComponent<Health>();
    statusEffects = GetComponent<StatusEffectReceiver>();
    animator = GetComponent<Animator>();

    meshRenderer = GetComponentInChildren<Renderer>();
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
    bool diedFrozen = statusEffects != null && statusEffects.IsFrozen;
    StartCoroutine(DeathRoutine(diedFrozen));
  }

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
