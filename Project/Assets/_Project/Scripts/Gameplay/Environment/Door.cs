using System.Collections;
using UnityEngine;

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

  void Awake()
  {
    audioSource = GetComponent<AudioSource>();

    closedRotation = door.transform.rotation;
    targetRotation = closedRotation;
  }

  public void SetLocked(bool locked)
  {
    if (isLocked == locked) return;

    isLocked = locked;

    if (isLocked)
    {
      CloseDoor();
    }
  }

  public void OnTriggered(Vector3 sourcePosition)
  {
    if (isOpen || isLocked) return;

    OpenDoor(sourcePosition);
  }

  void OpenDoor(Vector3 playerPosition)
  {
    isOpen = true;
    targetRotation = closedRotation * Quaternion.Euler(0f, openAngle * GetSwingSign(playerPosition), 0f);

    Rotate();
    PlaySFX(openSFX);
  }

  void CloseDoor()
  {
    isOpen = false;
    targetRotation = closedRotation;

    Rotate();
    PlaySFX(closeSFX);
  }

  void Rotate()
  {
    if (rotateCoroutine != null)
    {
      StopCoroutine(rotateCoroutine);
    }

    rotateCoroutine = StartCoroutine(CoroutineRotate());
  }

  void PlaySFX(AudioClip clip)
  {
    if (clip != null && audioSource != null)
    {
      audioSource.PlayOneShot(clip);
    }
  }

  float GetSwingSign(Vector3 playerPosition)
  {
    Vector3 toPlayer = playerPosition - transform.position;
    toPlayer.y = 0f;

    Vector3 doorForward = closedRotation * Vector3.forward;
    float dot = Vector3.Dot(doorForward, toPlayer);
    float sign = dot >= 0f ? -1f : 1f;

    return invertSwingDirection ? -sign : sign;
  }

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
