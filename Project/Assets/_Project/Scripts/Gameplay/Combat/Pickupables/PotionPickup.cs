using UnityEngine;

// Restores health when the player collects a potion and has room below maximum health.
public class PotionPickup : PickupBase
{
  [SerializeField] int healAmount = 2;
  [SerializeField] GameObject pickupVfxPrefab;

  // Heals the player and reports whether the potion can be collected.
  protected override bool TryApplyEffect(Collider player)
  {
    Health health = player.GetComponent<Health>();

    if (health == null || health.CurrentHealth >= health.MaxHealth) return false;

    health.Heal(healAmount);
    return true;
  }

  // Spawns the configured visual effect after the potion is collected.
  protected override void PlayPickupVfx() => SpawnVfx(pickupVfxPrefab);
}
