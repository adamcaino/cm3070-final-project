using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Soft-locks onto the nearest valid enemy every frame (range + FOV cone from the camera + line-of-sight
/// raycast, mirroring EnemyDetection's pattern) and lets the player toggle a hard lock onto it via input.
/// PlayerLocomotion and PlayerLockOnCamera read LockedTarget/IsLocked and the OnLockOn/OnLockOff events
/// rather than polling, so this is the single source of truth for lock state.
/// </summary>
public class TargetLockController : MonoBehaviour
{
  [Header("Input")]
  [SerializeField] InputActionReference lockTargetAction;
  [SerializeField] InputActionReference cycleTargetAction;

  [Header("Detection")]
  [SerializeField] LayerMask enemyMask;
  [SerializeField] LayerMask obstacleMask;
  [SerializeField, Min(0f)] float lockRange = 12f;
  [SerializeField, Min(0f)] float unlockRange = 15f;
  [SerializeField, Range(0f, 180f)] float maxLockAngle = 60f;
  [SerializeField] float eyeHeight = 1.5f;

  Transform cameraTransform;
  Health softTargetHealth;
  Health lockedHealth;

  readonly Collider[] overlapBuffer = new Collider[16];
  readonly HashSet<Health> candidateBuffer = new HashSet<Health>();
  readonly List<(Health health, float angle)> cycleBuffer = new List<(Health, float)>();

  public Transform SoftTarget { get; private set; }
  public Transform LockedTarget { get; private set; }
  public bool IsLocked => LockedTarget != null;

  public event Action<Transform> OnSoftTargetChanged;
  public event Action<Transform> OnLockOn;
  public event Action OnLockOff;

  void Awake()
  {
    cameraTransform = Camera.main != null ? Camera.main.transform : null;
  }

  void OnEnable()
  {
    if (lockTargetAction != null)
    {
      lockTargetAction.action.Enable();
      lockTargetAction.action.performed += HandleLockPressed;
    }

    if (cycleTargetAction != null)
    {
      cycleTargetAction.action.Enable();
      cycleTargetAction.action.performed += HandleCyclePressed;
    }
  }

  void OnDisable()
  {
    if (lockTargetAction != null)
    {
      lockTargetAction.action.performed -= HandleLockPressed;
      lockTargetAction.action.Disable();
    }

    if (cycleTargetAction != null)
    {
      cycleTargetAction.action.performed -= HandleCyclePressed;
      cycleTargetAction.action.Disable();
    }
  }

  void Update()
  {
    RefreshSoftTarget();
    ValidateLockedTarget();
  }

  void RefreshSoftTarget()
  {
    Health nearest = FindNearestCandidate();
    if (nearest == softTargetHealth)
    {
      return;
    }

    softTargetHealth = nearest;
    SoftTarget = nearest != null ? nearest.transform : null;
    OnSoftTargetChanged?.Invoke(SoftTarget);
  }

  void ValidateLockedTarget()
  {
    if (!IsLocked)
    {
      return;
    }

    if (lockedHealth.IsDead)
    {
      Unlock();
      return;
    }

    float sqrDistance = (lockedHealth.transform.position - transform.position).sqrMagnitude;
    if (sqrDistance > unlockRange * unlockRange)
    {
      Unlock();
    }
  }

  Health FindNearestCandidate()
  {
    CollectCandidates(candidateBuffer, lockRange);

    Health nearest = null;
    float nearestSqrDistance = float.MaxValue;

    foreach (Health candidate in candidateBuffer)
    {
      float sqrDistance = (candidate.transform.position - transform.position).sqrMagnitude;
      if (sqrDistance >= nearestSqrDistance)
      {
        continue;
      }

      nearest = candidate;
      nearestSqrDistance = sqrDistance;
    }

    return nearest;
  }

  void CollectCandidates(HashSet<Health> results, float range)
  {
    results.Clear();
    if (cameraTransform == null)
    {
      return;
    }

    int count = Physics.OverlapSphereNonAlloc(transform.position, range, overlapBuffer, enemyMask, QueryTriggerInteraction.Ignore);
    for (int i = 0; i < count; i++)
    {
      Health health = overlapBuffer[i].GetComponentInParent<Health>();
      if (health == null || health.IsDead)
      {
        continue;
      }

      if (!IsWithinLockCone(health.transform) || !HasLineOfSight(health.transform))
      {
        continue;
      }

      results.Add(health);
    }
  }

  bool IsWithinLockCone(Transform candidate)
  {
    Vector3 cameraForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up);
    Vector3 toCandidate = Vector3.ProjectOnPlane(candidate.position - transform.position, Vector3.up);
    if (cameraForward.sqrMagnitude < 0.0001f || toCandidate.sqrMagnitude < 0.0001f)
    {
      return false;
    }

    return Vector3.Angle(cameraForward, toCandidate) <= maxLockAngle;
  }

  bool HasLineOfSight(Transform candidate)
  {
    Vector3 eyePosition = transform.position + (Vector3.up * eyeHeight);
    Vector3 toCandidate = candidate.position - eyePosition;
    return !Physics.Raycast(eyePosition, toCandidate.normalized, toCandidate.magnitude, obstacleMask, QueryTriggerInteraction.Ignore);
  }

  void HandleLockPressed(InputAction.CallbackContext context)
  {
    if (IsLocked)
    {
      Unlock();
    }
    else if (softTargetHealth != null)
    {
      LockOn(softTargetHealth);
    }
  }

  void HandleCyclePressed(InputAction.CallbackContext context)
  {
    if (!IsLocked)
    {
      if (softTargetHealth != null)
      {
        LockOn(softTargetHealth);
      }

      return;
    }

    float direction = context.ReadValue<float>();
    if (Mathf.Approximately(direction, 0f))
    {
      return;
    }

    CycleTarget(direction > 0f ? 1 : -1);
  }

  void CycleTarget(int direction)
  {
    CollectCandidates(candidateBuffer, unlockRange);
    if (candidateBuffer.Count == 0)
    {
      return;
    }

    Vector3 cameraForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up);

    cycleBuffer.Clear();
    foreach (Health candidate in candidateBuffer)
    {
      Vector3 toCandidate = Vector3.ProjectOnPlane(candidate.transform.position - transform.position, Vector3.up);
      float angle = Vector3.SignedAngle(cameraForward, toCandidate, Vector3.up);
      cycleBuffer.Add((candidate, angle));
    }

    cycleBuffer.Sort((a, b) => a.angle.CompareTo(b.angle));

    int currentIndex = cycleBuffer.FindIndex(entry => entry.health == lockedHealth);
    int nextIndex = currentIndex < 0 ? 0 : (currentIndex + direction + cycleBuffer.Count) % cycleBuffer.Count;

    LockOn(cycleBuffer[nextIndex].health);
  }

  void LockOn(Health health)
  {
    lockedHealth = health;
    LockedTarget = health.transform;
    OnLockOn?.Invoke(LockedTarget);
  }

  void Unlock()
  {
    lockedHealth = null;
    LockedTarget = null;
    OnLockOff?.Invoke();
  }

  void OnDrawGizmosSelected()
  {
    Gizmos.color = Color.cyan;
    Gizmos.DrawWireSphere(transform.position, lockRange);

    if (SoftTarget != null)
    {
      Gizmos.color = Color.yellow;
      Gizmos.DrawSphere(SoftTarget.position + (Vector3.up * eyeHeight), 0.2f);
    }

    if (LockedTarget != null)
    {
      Gizmos.color = Color.red;
      Gizmos.DrawSphere(LockedTarget.position + (Vector3.up * eyeHeight), 0.25f);
    }
  }
}
