using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(AudioSource))]
public class PlayerAudio : MonoBehaviour
{
  [SerializeField] AudioClip[] hurtClips;
  [SerializeField] AudioClip deathClip;

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
    health.OnDied += HandleDied;
  }

  void OnDisable()
  {
    health.OnDamaged -= HandleDamaged;
    health.OnDied -= HandleDied;
  }

  void HandleDamaged(Vector3 hitPoint)
  {
    if (hurtClips == null || hurtClips.Length == 0) return;

    audioSource.PlayOneShot(hurtClips[Random.Range(0, hurtClips.Length)]);
  }

  void HandleDied()
  {
    if (deathClip == null) return;

    audioSource.PlayOneShot(deathClip);
  }
}
