using UnityEngine;

public class PotionPickup : PickupBase
{
  [SerializeField] int healAmount = 2;
  [SerializeField] GameObject pickupVfxPrefab;

  protected override bool TryApplyEffect(Collider player)
  {
    Health health = player.GetComponent<Health>();

    // Don't consume the potion if the player is already at full health.
    if (health == null || health.CurrentHealth >= health.MaxHealth) return false;

    health.Heal(healAmount);
    return true;
  }

  protected override void PlayPickupVfx() => SpawnVfx(pickupVfxPrefab);
}
