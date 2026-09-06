using System.Collections;
using UnityEngine;

// Opens and closes a rotating door in response to trigger events and lock state changes.
public class Door : MonoBehaviour, ITriggerable
{
  [Header("References")]
  [SerializeField] GameObject door;

  [Header("Rotation")]
  [SerializeField] float openAngle = 90f;
  [SerializeField, Min(0f)] float rotationSpeed = 180f;
  [SerializeField] bool invertSwingDirection;

  [Header("SFX")]
  [SerializeField] AudioClip openSFX;
  [SerializeField] AudioClip closeSFX;

  Quaternion closedRotation;
  Quaternion targetRotation;
  AudioSource audioSource;
  Coroutine rotateCoroutine;

  bool isOpen;
  bool isLocked;

  public bool IsLocked => isLocked;

  // Caches the audio source and records the door's closed rotation.
  void Awake()
  {
    audioSource = GetComponent<AudioSource>();

    closedRotation = door.transform.rotation;
    targetRotation = closedRotation;
  }

  // Updates the lock state and closes the door when locking it.
  public void SetLocked(bool locked)
  {
    if (isLocked == locked) return;

    isLocked = locked;

    if (isLocked)
    {
      CloseDoor();
    }
  }

  // Opens the door when it is closed and unlocked.
  public void OnTriggered(Vector3 sourcePosition)
  {
    if (isOpen || isLocked) return;

    OpenDoor(sourcePosition);
  }

  // Calculates the swing direction from the player position and starts opening the door.
  void OpenDoor(Vector3 playerPosition)
  {
    isOpen = true;
    targetRotation = closedRotation * Quaternion.Euler(0f, openAngle * GetSwingSign(playerPosition), 0f);

    Rotate();
    PlaySFX(openSFX);
  }

  // Sets the closed rotation as the target and starts closing the door.
  void CloseDoor()
  {
    isOpen = false;
    targetRotation = closedRotation;

    Rotate();
    PlaySFX(closeSFX);
  }

  // Replaces any active rotation coroutine with a new one.
  void Rotate()
  {
    if (rotateCoroutine != null)
    {
      StopCoroutine(rotateCoroutine);
    }

    rotateCoroutine = StartCoroutine(CoroutineRotate());
  }

  // Plays the supplied door sound when both the clip and audio source are available.
  void PlaySFX(AudioClip clip)
  {
    if (clip != null && audioSource != null)
    {
      audioSource.PlayOneShot(clip);
    }
  }

  // Determines which side of the door should swing toward the player.
  float GetSwingSign(Vector3 playerPosition)
  {
    Vector3 toPlayer = playerPosition - transform.position;
    toPlayer.y = 0f;

    Vector3 doorForward = closedRotation * Vector3.forward;
    float dot = Vector3.Dot(doorForward, toPlayer);
    float sign = dot >= 0f ? -1f : 1f;

    return invertSwingDirection ? -sign : sign;
  }

  // Rotates the door toward its target until the remaining angle is negligible.
  IEnumerator CoroutineRotate()
  {
    while (Quaternion.Angle(door.transform.rotation, targetRotation) > 0.01f)
    {
      door.transform.rotation = Quaternion.RotateTowards(door.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
      yield return null;
    }

    rotateCoroutine = null;
  }
}
