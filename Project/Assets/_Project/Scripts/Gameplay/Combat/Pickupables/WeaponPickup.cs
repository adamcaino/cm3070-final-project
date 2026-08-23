using UnityEngine;

public class WeaponPickup : PickupBase
{
  [SerializeField] WeaponData weaponData;

  protected override bool TryApplyEffect(Collider player)
  {
    PlayerWeapon playerWeapon = player.GetComponentInParent<PlayerWeapon>();
    if (playerWeapon == null || weaponData == null) return false;

    playerWeapon.Equip(weaponData);
    return true;
  }

  protected override void PlayPickupVfx() => SpawnVfx(weaponData != null ? weaponData.pickupVFX : null);
}
