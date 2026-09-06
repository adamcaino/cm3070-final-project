using UnityEngine;

// Spawns simple VFX when the attached Health component takes damage or heals.
[RequireComponent(typeof(Health))]
public class HitReactionVFX : MonoBehaviour
{
  [SerializeField] GameObject bloodHitVfxPrefab;
  [SerializeField] GameObject healVfxPrefab;
  [SerializeField] Vector3 healVfxOffset = new Vector3(0f, 2f, 0f);

  Health health;

  // Caches the Health component used by the event handlers.
  void Awake()
  {
    health = GetComponent<Health>();
  }

  // Subscribes to damage and healing events when the component becomes active.
  void OnEnable()
  {
    health.OnDamaged += HandleDamaged;
    health.OnHealed += HandleHealed;
  }

  // Removes the damage and healing event subscriptions when the component is disabled.
  void OnDisable()
  {
    health.OnDamaged -= HandleDamaged;
    health.OnHealed -= HandleHealed;
  }

  // Spawns the configured blood effect at the point where damage was received.
  void HandleDamaged(Vector3 hitPoint)
  {
    if (bloodHitVfxPrefab == null) return;

    Instantiate(bloodHitVfxPrefab, hitPoint, Quaternion.identity);
  }

  // Spawns the configured healing effect above the reported healing point.
  void HandleHealed(Vector3 healPoint)
  {
    if (healVfxPrefab == null) return;

    Instantiate(healVfxPrefab, healPoint + healVfxOffset, Quaternion.identity);
  }
}
