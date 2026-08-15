using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour, ITriggerable
{
  [Header("Rotation")]
  [SerializeField] float openAngle = 90f;
  [SerializeField, Min(0f)] float rotationSpeed = 180f;
  [Tooltip("Flip this if the door swings towards the player instead of away - depends on which way the door's forward axis faces.")]
  [SerializeField] bool invertSwingDirection;

  [Header("SFX")]
  [SerializeField] AudioClip openSFX;

  Quaternion closedRotation;
  Quaternion targetRotation;

  bool isOpen;

  void Awake()
  {
    closedRotation = transform.rotation;
    targetRotation = closedRotation;
  }

  public void OnTriggered(Vector3 sourcePosition)
  {
    if (isOpen) return; // Ensure the door doesn't open again while the player is still triggering it.

    OpenDoor(sourcePosition);
  }

  void OpenDoor(Vector3 playerPosition)
  {
    isOpen = true;
    targetRotation = closedRotation * Quaternion.Euler(0f, openAngle * GetSwingSign(playerPosition), 0f);

    // Start the coroutine to smoothly rotate the door open
    StartCoroutine(CoroutineRotateOpen());

    // Play the open sound effect if it is assigned
    if (openSFX != null)
    {
      AudioSource.PlayClipAtPoint(openSFX, transform.position);
    }
  }

  // Swings the door away from whichever side the player is standing on, using the door's
  // closed-state forward axis as the dividing plane between "front" and "back".
  float GetSwingSign(Vector3 playerPosition)
  {
    Vector3 toPlayer = playerPosition - transform.position;
    toPlayer.y = 0f;

    Vector3 doorForward = closedRotation * Vector3.forward;
    float dot = Vector3.Dot(doorForward, toPlayer);
    float sign = dot >= 0f ? -1f : 1f;

    return invertSwingDirection ? -sign : sign;
  }

  // Coroutine to smoothly rotate the door open
  IEnumerator CoroutineRotateOpen()
  {
    while (Quaternion.Angle(transform.rotation, targetRotation) > 0.01f)
    {
      transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
      yield return null;
    }
  }
}
