using UnityEngine;

[RequireComponent(typeof(CharacterController))]
// Plays footstep sounds after the player travels the configured distance while grounded.
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
  ScriptedForwardWalker scriptedWalker;
  AudioSource audioSource;
  float distanceSinceLastStep;
  int lastClipIndex = -1;

  // Caches movement, controller, and audio components used by the footstep system.
  void Awake()
  {
    controller = GetComponent<CharacterController>();
    locomotion = GetComponent<PlayerLocomotion>();
    scriptedWalker = GetComponent<ScriptedForwardWalker>();
    audioSource = GetComponent<AudioSource>();
  }

  // Accumulates grounded movement distance and plays a step when the threshold is reached.
  void Update()
  {
    if (footstepClips == null || footstepClips.Length == 0)
    {
      return;
    }

    float currentSpeed = locomotion != null && locomotion.enabled
      ? locomotion.CurrentSpeed
      : scriptedWalker != null ? scriptedWalker.CurrentSpeed : 0f;
    if (!controller.isGrounded || currentSpeed < minMoveSpeed)
    {
      distanceSinceLastStep = 0f;
      return;
    }

    distanceSinceLastStep += currentSpeed * Time.deltaTime;

    if (distanceSinceLastStep >= stepDistance)
    {
      distanceSinceLastStep = 0f;
      PlayFootstep();
    }
  }

  // Selects a clip, varies its pitch, and plays the footstep sound.
  void PlayFootstep()
  {
    int index = footstepClips.Length == 1 ? 0 : RandomIndexExcluding(lastClipIndex);
    lastClipIndex = index;

    audioSource.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
    audioSource.PlayOneShot(footstepClips[index], volume);
  }

  // Selects a random clip index different from the previous clip when possible.
  int RandomIndexExcluding(int excluded)
  {
    int index = Random.Range(0, footstepClips.Length);
    return index == excluded ? (index + 1) % footstepClips.Length : index;
  }
}
