using Unity.Cinemachine;
using UnityEngine;

public class PlayerLockOnCamera : MonoBehaviour
{
  [Header("References")]
  [SerializeField] CinemachineCamera orbitalVcam;
  [SerializeField] CinemachineCamera lockOnVcam;
  TargetLockController targetLock;

  [Header("Priority")]
  [Tooltip("Added to the orbital vcam's own priority so the lock-on camera always loses to it while free.")]
  [SerializeField] int freePriorityOffset = -1;
  [Tooltip("Added to the orbital vcam's own priority so the lock-on camera always wins over it while locked.")]
  [SerializeField] int lockedPriorityOffset = 20;

  int basePriority;

  void Awake()
  {
    targetLock = GetComponent<TargetLockController>();

    basePriority = orbitalVcam != null ? orbitalVcam.Priority.Value : 0;

    if (lockOnVcam != null)
    {
      lockOnVcam.Priority = basePriority + freePriorityOffset;
    }
  }

  void OnEnable()
  {
    if (targetLock == null)
    {
      return;
    }

    targetLock.OnLockOn += HandleLockOn;
    targetLock.OnLockOff += HandleLockOff;
  }

  void OnDisable()
  {
    if (targetLock == null)
    {
      return;
    }

    targetLock.OnLockOn -= HandleLockOn;
    targetLock.OnLockOff -= HandleLockOff;
  }

  void HandleLockOn(Transform target)
  {
    if (lockOnVcam == null)
    {
      return;
    }

    lockOnVcam.LookAt = target;
    lockOnVcam.Priority = basePriority + lockedPriorityOffset;
  }

  void HandleLockOff()
  {
    if (lockOnVcam == null)
    {
      return;
    }

    lockOnVcam.LookAt = null;
    lockOnVcam.Priority = basePriority + freePriorityOffset;
  }
}
