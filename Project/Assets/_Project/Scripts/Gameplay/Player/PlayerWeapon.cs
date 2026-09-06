using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
// Equips weapons, controls their hitbox, plays swing effects, and applies hit effects.
public class PlayerWeapon : MonoBehaviour
{
  [SerializeField] WeaponData defaultWeapon;
  [SerializeField] Transform weaponSocket;

  [Header("Swing VFX placement")]
  [Tooltip("Distance in front of the player to spawn the swing VFX.")]
  [SerializeField] float swingVfxForwardOffset = 1.2f;
  [Tooltip("Height above the player's feet to spawn the swing VFX.")]
  [SerializeField] float swingVfxHeightOffset = 1.1f;
  [Tooltip("Extra rotation (degrees) applied to the swing VFX on top of the player's facing.")]
  [SerializeField] float swingVfxRotationX;
  [SerializeField] float swingVfxRotationZ;

  GameObject equippedInstance;
  WeaponHitbox equippedHitbox;
  AudioSource audioSource;
  AudioSource ambientAudioSource;
  bool socketCleared;

  public WeaponData Current { get; private set; }
  public int RemainingUses { get; private set; }

  public event Action<WeaponData> OnWeaponChanged;
  public event Action<int, int> OnManaChanged;

  // Caches the primary audio source and creates the looping ambient audio source.
  void Awake()
  {
    audioSource = GetComponent<AudioSource>();

    ambientAudioSource = gameObject.AddComponent<AudioSource>();
    ambientAudioSource.loop = true;
    ambientAudioSource.playOnAwake = false;
  }

  // Equips the configured default weapon after initialization.
  void Start()
  {
    if (defaultWeapon != null)
    {
      Equip(defaultWeapon);
    }
  }

  // Replaces the current weapon, subscribes its hitbox, updates uses, and raises UI events.
  public void Equip(WeaponData weapon)
  {
    if (weapon == null || weaponSocket == null)
    {
      return;
    }

    if (!socketCleared)
    {
      for (int i = weaponSocket.childCount - 1; i >= 0; i--)
      {
        weaponSocket.GetChild(i).gameObject.SetActive(false);
      }
      socketCleared = true;
    }

    if (equippedHitbox != null)
    {
      equippedHitbox.OnHit -= HandleHit;
    }

    if (equippedInstance != null)
    {
      Destroy(equippedInstance);
    }

    equippedInstance = Instantiate(weapon.weaponPrefab, weaponSocket);
    equippedHitbox = equippedInstance.GetComponentInChildren<WeaponHitbox>();

    if (equippedHitbox != null)
    {
      equippedHitbox.OnHit += HandleHit;
    }

    Current = weapon;
    RemainingUses = weapon.maxUses;

    UpdateAmbience(weapon);

    OnWeaponChanged?.Invoke(weapon);
    OnManaChanged?.Invoke(RemainingUses, weapon.maxUses);
  }

  // Starts or stops the ambient audio configured by the weapon data.
  void UpdateAmbience(WeaponData weapon)
  {
    if (weapon.ambientSfxClip == null)
    {
      ambientAudioSource.Stop();
      ambientAudioSource.clip = null;
      return;
    }

    ambientAudioSource.clip = weapon.ambientSfxClip;
    ambientAudioSource.volume = weapon.ambientVolume;
    ambientAudioSource.Play();
  }

  // Enables the equipped weapon hitbox during an attack animation.
  public void EnableHitbox() => equippedHitbox?.EnableHitbox();

  // Disables the equipped weapon hitbox after an attack animation.
  public void DisableHitbox() => equippedHitbox?.DisableHitbox();

  // Spawns the swing visual effect and plays its sound using the requested mirror state.
  public void PlaySwingVFX(float mirror = 0f)
  {
    if (Current == null)
    {
      return;
    }

    if (Current.swingVfxPrefab != null)
    {
      Vector3 vfxPosition = transform.position + transform.forward * swingVfxForwardOffset + transform.up * swingVfxHeightOffset;

      bool isMirrored = mirror > 0.5f;
      float mirrorYaw = isMirrored ? 0f : 180f;
      float rotationX = isMirrored ? -swingVfxRotationX : swingVfxRotationX;

      Quaternion vfxRotation = transform.rotation * Quaternion.Euler(rotationX, mirrorYaw, swingVfxRotationZ);
      Instantiate(Current.swingVfxPrefab, vfxPosition, vfxRotation);
    }

    if (Current.swingSfxClip != null)
    {
      audioSource.PlayOneShot(Current.swingSfxClip);
    }
  }

  // Applies weapon damage, optional affliction, and use consumption to a hit target.
  void HandleHit(Collider other)
  {
    if (Current == null) return;

    IDamageable damageable = other.GetComponentInParent<IDamageable>();
    if (damageable == null) return;

    Vector3 hitPoint = other.ClosestPoint(equippedHitbox.transform.position);
    damageable.TakeDamage(Current.damage, gameObject, hitPoint);

    if (Current.afflictionType != AfflictionType.None)
    {
      IAfflictable afflictable = other.GetComponentInParent<IAfflictable>();
      afflictable?.ApplyAffliction(Current.afflictionType, Current.afflictionDuration, Current.afflictionMagnitude, gameObject);
    }

    ConsumeUse();
  }

  // Decrements finite weapon uses and returns to the default weapon when exhausted.
  void ConsumeUse()
  {
    if (!Current.IsSpecial) return;

    RemainingUses = Mathf.Max(0, RemainingUses - 1);
    OnManaChanged?.Invoke(RemainingUses, Current.maxUses);

    if (RemainingUses <= 0)
    {
      Equip(defaultWeapon);
    }
  }
}
