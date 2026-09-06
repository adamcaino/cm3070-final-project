using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Combat/Weapon")]
// Stores the combat, visual, audio, and affliction settings used by a weapon.
public class WeaponData : ScriptableObject
{
  public string weaponName;
  public GameObject weaponPrefab;

  [Min(0)] public int damage = 1;

  [Header("Affliction (optional)")]
  public AfflictionType afflictionType;
  [Min(0)] public int afflictionMagnitude;
  [Min(0f)] public float afflictionDuration;

  [Header("Swing")]
  public GameObject swingVfxPrefab;
  public AudioClip swingSfxClip;

  [Header("Ambient (optional)")]
  public AudioClip ambientSfxClip;
  [Range(0f, 1f)] public float ambientVolume = 0.3f;

  [Header("Pickup Effect")]
  public GameObject pickupVFX;

  [Header("Special Weapon (optional)")]
  [Tooltip("Number of successful hits before the weapon breaks and the player reverts to their default weapon. 0 = unlimited (not a special weapon).")]
  [Min(0)] public int maxUses = 0;
  public Sprite icon;

  // Reports whether the weapon has a finite number of successful uses.
  public bool IsSpecial => maxUses > 0;
}
