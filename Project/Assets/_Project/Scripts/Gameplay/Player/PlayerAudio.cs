using UnityEngine;

/// <summary>
/// Player-side counterpart to EnemyAudio - reacts to the player's own Health.OnDamaged with a hurt SFX.
/// Kept separate from HitReactionVFX/HitFlash since those are generic, shared components that also run
/// on enemies; this is deliberately player-only, same reasoning that split EnemyAudio out for enemies.
/// </summary>
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(AudioSource))]
public class PlayerAudio : MonoBehaviour
{
  [SerializeField] AudioClip[] hurtClips;

  Health health;
  AudioSource audioSource;

  void Awake()
  {
    health = GetComponent<Health>();
    audioSource = GetComponent<AudioSource>();
  }

  void OnEnable()
  {
    health.OnDamaged += HandleDamaged;
  }

  void OnDisable()
  {
    health.OnDamaged -= HandleDamaged;
  }

  void HandleDamaged(Vector3 hitPoint)
  {
    if (hurtClips == null || hurtClips.Length == 0) return;

    audioSource.PlayOneShot(hurtClips[Random.Range(0, hurtClips.Length)]);
  }
}
