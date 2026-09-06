using UnityEngine;






[RequireComponent(typeof(TargetLockController))]
[RequireComponent(typeof(AudioSource))]
// Plays a sound when the player acquires a lock-on target.
public class PlayerLockOnAudio : MonoBehaviour
{
  [SerializeField] AudioClip toggleLockOnClip;

  TargetLockController lockController;
  AudioSource audioSource;

  // Caches the target-lock controller and audio source.
  void Awake()
  {
    lockController = GetComponent<TargetLockController>();
    audioSource = GetComponent<AudioSource>();
  }

  // Subscribes to the lock-on event.
  void OnEnable()
  {
    lockController.OnLockOn += HandleLockOn;
  }

  // Removes the lock-on event subscription.
  void OnDisable()
  {
    lockController.OnLockOn -= HandleLockOn;
  }

  // Plays the configured lock-on sound.
  void HandleLockOn(Transform _)
  {
    if (toggleLockOnClip == null) return;

    audioSource.PlayOneShot(toggleLockOnClip);
  }
}
