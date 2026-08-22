using UnityEngine;

public class PotionPickup : PickupBase
{
  [SerializeField] int healAmount = 1;
  [SerializeField] GameObject pickupVfxPrefab;
  [SerializeField] AudioClip pickupSfxClip;

  protected override bool TryApplyEffect(Collider player)
  {
    Health health = player.GetComponent<Health>();

    // Don't consume the potion if the player is already at full health.
    if (health == null || health.CurrentHealth >= health.MaxHealth) return false;

    health.Heal(healAmount);
    return true;
  }

  protected override void PlayPickupVfx() => SpawnVfx(pickupVfxPrefab);
  protected override void PlayPickupSfx() => PlaySfx(pickupSfxClip);
}
