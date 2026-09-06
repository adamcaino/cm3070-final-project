using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(AudioSource))]
// Plays player hurt and death sounds in response to Health events.
public class PlayerAudio : MonoBehaviour
{
  [SerializeField] AudioClip[] hurtClips;
  [SerializeField] AudioClip deathClip;

  Health health;
  AudioSource audioSource;

  // Caches the health and audio components used by the event handlers.
  void Awake()
  {
    health = GetComponent<Health>();
    audioSource = GetComponent<AudioSource>();
  }

  // Subscribes to damage and death events.
  void OnEnable()
  {
    health.OnDamaged += HandleDamaged;
    health.OnDied += HandleDied;
  }

  // Removes damage and death event subscriptions.
  void OnDisable()
  {
    health.OnDamaged -= HandleDamaged;
    health.OnDied -= HandleDied;
  }

  // Plays a random hurt clip when the player takes damage.
  void HandleDamaged(Vector3 hitPoint)
  {
    if (hurtClips == null || hurtClips.Length == 0) return;

    audioSource.PlayOneShot(hurtClips[Random.Range(0, hurtClips.Length)]);
  }

  // Plays the configured death clip when the player dies.
  void HandleDied()
  {
    if (deathClip == null) return;

    audioSource.PlayOneShot(deathClip);
  }
}
