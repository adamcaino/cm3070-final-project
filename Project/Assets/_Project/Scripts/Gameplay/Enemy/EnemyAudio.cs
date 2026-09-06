using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(AudioSource))]
public class EnemyAudio : MonoBehaviour
{
  [Header("Combat")]
  [SerializeField] AudioClip[] alertClips;
  [SerializeField] AudioClip[] attackClips;
  [SerializeField] AudioClip[] hitClips;
  [SerializeField] AudioClip[] deathClips;

  [Header("Flying")]
  [SerializeField] AudioSource flyingAudioSource;
  [SerializeField] AudioClip flyingOneShotClip;

  Health health;
  AudioSource audioSource;

  // Caches the health and audio components used by event handlers and public controls.
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

  #region Events

  // Plays a random hit sound when the enemy takes damage.
  void HandleDamaged(Vector3 hitPoint) => PlayRandom(hitClips);

  // Plays a random death sound when the enemy dies.
  void HandleDied() => PlayRandom(deathClips);

  #endregion

  // Selects and plays a random clip from the supplied collection.
  void PlayRandom(AudioClip[] clips)
  {
    if (clips == null || clips.Length == 0) return;

    audioSource.pitch = Random.Range(0.8f, 1.2f); ;
    audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
  }

  #region Public Functions

  // Plays a random alert sound.
  public void PlayAlertSfx() => PlayRandom(alertClips);

  // Plays a random attack sound.
  public void PlayAttackSfx() => PlayRandom(attackClips);

  // Plays a random hit sound.
  public void PlayHitSfx() => PlayRandom(hitClips);

  // Stops the looping flight sound.
  public void StopFlightSfx() => flyingAudioSource.Stop();

  // Plays the configured flight sound as a one-shot.
  public void PlayFlightSfx()
  {
    if (flyingOneShotClip != null)
    {
      flyingAudioSource.PlayOneShot(flyingOneShotClip);
    }
  }

  #endregion
}
