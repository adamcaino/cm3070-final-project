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
  [SerializeField, Min(0f)] float deathDelay = 1.5f;
  [SerializeField, Min(0f)] float vfxLifetime = 3.5f;

  Health health;
  Renderer meshRenderer;

  void Awake()
  {
    health = GetComponent<Health>();

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
    StartCoroutine(DeathRoutine());
  }

  IEnumerator DeathRoutine()
  {
    yield return new WaitForSeconds(deathDelay);

    if (deathVfxPrefab != null)
    {
      Vector3 spawnPosition = meshRenderer != null ? meshRenderer.bounds.center : transform.position;
      GameObject vfx = Instantiate(deathVfxPrefab, spawnPosition, transform.rotation);
      Destroy(vfx, vfxLifetime);
    }

    Destroy(gameObject);
  }
}
