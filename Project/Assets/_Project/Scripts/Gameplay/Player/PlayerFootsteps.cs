using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerLocomotion))]
public class PlayerFootsteps : MonoBehaviour
{
  [SerializeField] AudioClip[] footstepClips;
  [Tooltip("Distance in meters the player must travel between footstep sounds.")]
  [SerializeField, Min(0.1f)] float stepDistance = 2f;
  [SerializeField, Range(0f, 1f)] float volume = 0.6f;
  [SerializeField, Range(0f, 0.5f)] float pitchVariance = 0.1f;
  [SerializeField, Min(0f)] float minMoveSpeed = 0.1f;

  CharacterController controller;
  PlayerLocomotion locomotion;
  AudioSource audioSource;
  float distanceSinceLastStep;
  int lastClipIndex = -1;

  void Awake()
  {
    controller = GetComponent<CharacterController>();
    locomotion = GetComponent<PlayerLocomotion>();
    audioSource = GetComponent<AudioSource>();
  }

  void Update()
  {
    if (footstepClips == null || footstepClips.Length == 0)
    {
      return;
    }

    if (!controller.isGrounded || locomotion.CurrentSpeed < minMoveSpeed)
    {
      distanceSinceLastStep = 0f;
      return;
    }

    distanceSinceLastStep += locomotion.CurrentSpeed * Time.deltaTime;

    if (distanceSinceLastStep >= stepDistance)
    {
      distanceSinceLastStep = 0f;
      PlayFootstep();
    }
  }

  void PlayFootstep()
  {
    int index = footstepClips.Length == 1 ? 0 : RandomIndexExcluding(lastClipIndex);
    lastClipIndex = index;

    audioSource.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
    audioSource.PlayOneShot(footstepClips[index], volume);
  }

  int RandomIndexExcluding(int excluded)
  {
    int index = Random.Range(0, footstepClips.Length);
    return index == excluded ? (index + 1) % footstepClips.Length : index;
  }
}
