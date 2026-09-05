using UnityEngine;






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
