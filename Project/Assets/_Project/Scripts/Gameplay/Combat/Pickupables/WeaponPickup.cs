using UnityEngine;

// Equips the configured weapon data when the player collects the pickup.
public class WeaponPickup : PickupBase
{
  [SerializeField] WeaponData weaponData;

  // Equips the weapon and reports whether the pickup has the required references.
  protected override bool TryApplyEffect(Collider player)
  {
    PlayerWeapon playerWeapon = player.GetComponentInParent<PlayerWeapon>();
    if (playerWeapon == null || weaponData == null) return false;

    playerWeapon.Equip(weaponData);

    if (weaponData.IsSpecial)
    {
      SpecialWeaponTutorialPrompt.ShowForPickup(weaponData);
    }

    return true;
  }

  // Spawns the visual effect configured by the weapon data after collection.
  protected override void PlayPickupVfx() => SpawnVfx(weaponData != null ? weaponData.pickupVFX : null);
}
