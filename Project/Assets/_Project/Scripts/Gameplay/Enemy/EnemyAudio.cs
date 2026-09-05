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

  #region Events

  void HandleDamaged(Vector3 hitPoint) => PlayRandom(hitClips);
  void HandleDied() => PlayRandom(deathClips);

  #endregion

  void PlayRandom(AudioClip[] clips)
  {
    if (clips == null || clips.Length == 0) return;

    audioSource.pitch = Random.Range(0.8f, 1.2f); ;
    audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
  }

  #region Public Functions

  public void PlayAlertSfx() => PlayRandom(alertClips);
  public void PlayAttackSfx() => PlayRandom(attackClips);
  public void PlayHitSfx() => PlayRandom(hitClips);
  public void StopFlightSfx() => flyingAudioSource.Stop();

  public void PlayFlightSfx()
  {
    if (flyingOneShotClip != null)
    {
      flyingAudioSource.PlayOneShot(flyingOneShotClip);
    }
  }

  #endregion
}
