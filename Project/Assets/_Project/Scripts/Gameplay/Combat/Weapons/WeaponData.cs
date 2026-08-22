using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Combat/Weapon")]
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
  public AudioClip pickupSFX;
}
