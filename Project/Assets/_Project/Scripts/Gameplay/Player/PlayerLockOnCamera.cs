using Unity.Cinemachine;
using UnityEngine;

// Switches Cinemachine camera priority and target assignment when lock-on changes.
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

  // Resolves camera and target-lock references during initialization.
  void Awake()
  {
    RefreshRuntimeReferences();
  }

  // Refreshes references and subscribes to lock-on events.
  void OnEnable()
  {
    RefreshRuntimeReferences();

    if (targetLock == null)
    {
      return;
    }

    targetLock.OnLockOn += HandleLockOn;
    targetLock.OnLockOff += HandleLockOff;
  }

  // Removes lock-on event subscriptions.
  void OnDisable()
  {
    if (targetLock == null)
    {
      return;
    }

    targetLock.OnLockOn -= HandleLockOn;
    targetLock.OnLockOff -= HandleLockOff;
  }

  // Resolves the free and lock-on cameras and applies their initial priorities.
  public void RefreshRuntimeReferences()
  {
    targetLock = GetComponent<TargetLockController>();

    if (orbitalVcam == null)
    {
      CinemachineOrbitalFollow orbitalFollow = PlayerCameraOrbit.ResolveGameplayOrbitalFollow();
      orbitalVcam = orbitalFollow != null ? orbitalFollow.GetComponent<CinemachineCamera>() : null;
    }

    if (lockOnVcam == null)
    {
      CinemachineCamera[] cameras = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
      foreach (CinemachineCamera camera in cameras)
      {
        if (camera == null || camera == orbitalVcam || camera.GetComponent<PlayerDeathCamera>() != null)
        {
          continue;
        }

        lockOnVcam = camera;
        break;
      }
    }

    basePriority = orbitalVcam != null ? orbitalVcam.Priority.Value : 0;

    // Ensure orbital camera has higher priority than lock-on camera when not locked.
    if (orbitalVcam != null && orbitalVcam.Priority.Value <= 0)
    {
      orbitalVcam.Priority = 10;
      basePriority = 10;
    }

    if (lockOnVcam == null)
    {
      return;
    }

    lockOnVcam.Follow = transform.Find("CameraTargetPos") ?? transform;

    // Set lock-on camera to lose to orbital when free, and win only when player is locked.
    if (targetLock != null && targetLock.IsLocked)
    {
      lockOnVcam.LookAt = targetLock.LockedTarget;
      lockOnVcam.Priority = basePriority + lockedPriorityOffset;
    }
    else
    {
      lockOnVcam.LookAt = null;
      lockOnVcam.Priority = basePriority + freePriorityOffset;
    }
  }

  // Assigns the locked target and raises the lock-on camera priority.
  void HandleLockOn(Transform target)
  {
    if (lockOnVcam == null)
    {
      return;
    }

    lockOnVcam.PreviousStateIsValid = false;
    lockOnVcam.LookAt = target;
    lockOnVcam.Priority = basePriority + lockedPriorityOffset;
  }

  // Clears the lock-on target and returns the camera to its free priority.
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
