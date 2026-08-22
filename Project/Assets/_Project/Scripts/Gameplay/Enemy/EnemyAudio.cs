using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(AudioSource))]
public class EnemyAudio : MonoBehaviour
{
  [Header("Combat")]
  [SerializeField] AudioClip[] attackClips;
  [SerializeField] AudioClip[] hitClips;
  [SerializeField] AudioClip[] deathClips;

  [Header("Flying (optional - Bat/Dragon only)")]
  [SerializeField] AudioClip flightLoopClip;

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

  // Called directly from AttackBase - see PlayAttackAnimation.
  public void PlayAttackSfx() => PlayRandom(attackClips);

  // Called via Animation Event on any clip where the wings should be flapping
  public void PlayFlightSfx()
  {
    if (flightLoopClip != null)
    {
      audioSource.PlayOneShot(flightLoopClip);
    }
  }

  void HandleDamaged(Vector3 hitPoint) => PlayRandom(hitClips);

  void HandleDied() => PlayRandom(deathClips);

  void PlayRandom(AudioClip[] clips)
  {
    if (clips == null || clips.Length == 0) return;

    audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
  }
}
