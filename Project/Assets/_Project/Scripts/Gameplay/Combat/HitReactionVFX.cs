using UnityEngine;

// Spawns simple VFX when the attached Health component takes damage or heals.
[RequireComponent(typeof(Health))]
public class HitReactionVFX : MonoBehaviour
{
  [SerializeField] GameObject bloodHitVfxPrefab;
  [SerializeField] GameObject healVfxPrefab;
  [SerializeField] Vector3 healVfxOffset = new Vector3(0f, 2f, 0f);

  Health health;

  void Awake()
  {
    health = GetComponent<Health>();
  }

  void OnEnable()
  {
    health.OnDamaged += HandleDamaged;
    health.OnHealed += HandleHealed;
  }

  void OnDisable()
  {
    health.OnDamaged -= HandleDamaged;
    health.OnHealed -= HandleHealed;
  }

  void HandleDamaged(Vector3 hitPoint)
  {
    if (bloodHitVfxPrefab == null) return;

    Instantiate(bloodHitVfxPrefab, hitPoint, Quaternion.identity);
  }

  void HandleHealed(Vector3 healPoint)
  {
    if (healVfxPrefab == null) return;

    Instantiate(healVfxPrefab, healPoint + healVfxOffset, Quaternion.identity);
  }
}
