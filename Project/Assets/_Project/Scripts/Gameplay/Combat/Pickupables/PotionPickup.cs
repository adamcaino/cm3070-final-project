using UnityEngine;

public class PotionPickup : PickupBase
{
  [SerializeField] int healAmount = 2;
  [SerializeField] GameObject pickupVfxPrefab;

  protected override bool TryApplyEffect(Collider player)
  {
    Health health = player.GetComponent<Health>();

    if (health == null || health.CurrentHealth >= health.MaxHealth) return false;

    health.Heal(healAmount);
    return true;
  }

  protected override void PlayPickupVfx() => SpawnVfx(pickupVfxPrefab);
}
