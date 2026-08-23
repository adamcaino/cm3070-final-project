using UnityEngine;

/// <summary>
/// Plays a toggle SFX whenever TargetLockController's OnLockOn fires - covers both a fresh lock
/// (space) and cycling onto a different target (q/e), since CycleTarget routes through LockOn too.
/// OnLockOff deliberately doesn't trigger this; unlocking has no sound per design.
/// </summary>
[RequireComponent(typeof(TargetLockController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerLockOnAudio : MonoBehaviour
{
  [SerializeField] AudioClip toggleLockOnClip;

  TargetLockController lockController;
  AudioSource audioSource;

  void Awake()
  {
    lockController = GetComponent<TargetLockController>();
    audioSource = GetComponent<AudioSource>();
  }

  void OnEnable()
  {
    lockController.OnLockOn += HandleLockOn;
  }

  void OnDisable()
  {
    lockController.OnLockOn -= HandleLockOn;
  }

  void HandleLockOn(Transform _)
  {
    if (toggleLockOnClip == null) return;

    audioSource.PlayOneShot(toggleLockOnClip);
  }
}
