using System.Collections;
using UnityEngine;

/// <summary>
/// Reacts to Health.OnDied the same decoupled way EnemyLootDrop does, rather than folding this into
/// DeadState - the state machine only owns animation/AI concerns, not how long the corpse lingers or
/// what marks its removal. Lets the Die animation read for a beat, then plays the death VFX in its
/// place and removes the enemy. The VFX prefab's own particle systems don't self-destroy (stopAction
/// is None on all of them), so this destroys the spawned instance itself after vfxLifetime.
/// </summary>
[RequireComponent(typeof(Health))]
public class EnemyDeathVFX : MonoBehaviour
{
  [SerializeField] GameObject deathVfxPrefab;
  [SerializeField] GameObject frozenDeathVfxPrefab;
  [SerializeField, Min(0f)] float deathDelay = 1.5f;
  [SerializeField, Min(0f)] float vfxLifetime = 3.5f;

  Health health;
  StatusEffectReceiver statusEffects;
  Renderer meshRenderer;

  void Awake()
  {
    health = GetComponent<Health>();
    statusEffects = GetComponent<StatusEffectReceiver>();

    // The death animation (e.g. falling backward) can carry the posed mesh well away from the root
    // transform that AI/NavMesh use - spawn at the renderer's actual world bounds instead of
    // transform.position so the VFX lands where the enemy visually ended up, not its logical position.
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
    // A frozen death has no animation to wait on (DeadState skips it, the Animator's paused anyway) -
    // swap straight to the frozen prefab instead of waiting for a Die animation that never plays.
    // Still yield at least one frame rather than destroying inline: Health.OnDied fires every
    // subscriber synchronously in registration order, and EnemyController's handler (which disables
    // the NavMeshAgent via DeadState) may not have run yet if this component happens to be earlier in
    // that order - destroying the GameObject before that cleanup runs left the agent's native handle
    // torn down while EnemyController was still mid-transition, throwing from NavMeshAgent internals.
    if (!diedFrozen)
    {
      yield return new WaitForSeconds(deathDelay);
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
