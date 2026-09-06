using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;







// Finds, selects, validates, and cycles targets within the player's lock-on view.
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
  readonly HashSet<Health> cycleCandidateBuffer = new HashSet<Health>();
  readonly List<(Health health, float angle)> cycleBuffer = new List<(Health, float)>();

  public Transform SoftTarget { get; private set; }
  public Transform LockedTarget { get; private set; }
  public bool IsLocked => LockedTarget != null;






  public HashSet<Health> RangeCandidates => candidateBuffer;

  public event Action<Transform> OnSoftTargetChanged;
  public event Action<Transform> OnLockOn;
  public event Action OnLockOff;

  // Resolves the camera reference used by target selection.
  void Awake()
  {
    RefreshRuntimeReferences();
  }

  // Refreshes the camera reference after runtime player setup.
  public void RefreshRuntimeReferences()
  {
    cameraTransform = Camera.main != null ? Camera.main.transform : null;
  }

  // Enables lock and cycle input actions and subscribes to their callbacks.
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

  // Removes input callbacks and disables lock and cycle actions.
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

  // Refreshes the soft target and validates the current locked target.
  void Update()
  {
    RefreshSoftTarget();
    ValidateLockedTarget();
  }

  // Finds the nearest valid candidate and raises the soft-target event when it changes.
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

  // Unlocks when the target dies or moves beyond the configured unlock range.
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

  // Returns the nearest valid target within the lock range.
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

  // Fills a result set with living targets inside the range and line of sight.
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

  // Tests whether a candidate lies inside the camera-relative lock cone.
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

  // Tests whether an obstacle blocks the player's view of a candidate.
  bool HasLineOfSight(Transform candidate)
  {
    Vector3 eyePosition = transform.position + (Vector3.up * eyeHeight);
    Vector3 toCandidate = candidate.position - eyePosition;
    return !Physics.Raycast(eyePosition, toCandidate.normalized, toCandidate.magnitude, obstacleMask, QueryTriggerInteraction.Ignore);
  }

  // Toggles lock-on for the soft target when the lock action is pressed.
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

  // Locks the soft target or cycles through candidates from the axis input.
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

  // Sorts candidates by signed camera angle and selects the next target.
  void CycleTarget(int direction)
  {
    CollectCandidates(cycleCandidateBuffer, unlockRange);
    if (cycleCandidateBuffer.Count == 0)
    {
      return;
    }

    Vector3 cameraForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up);

    cycleBuffer.Clear();
    foreach (Health candidate in cycleCandidateBuffer)
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

  // Stores the selected target and raises the lock-on event.
  void LockOn(Health health)
  {
    lockedHealth = health;
    LockedTarget = health.transform;
    OnLockOn?.Invoke(LockedTarget);
  }

  // Clears the selected target and raises the lock-off event.
  void Unlock()
  {
    lockedHealth = null;
    LockedTarget = null;
    OnLockOff?.Invoke();
  }

  // Draws the lock range and current soft and locked targets in the editor.
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
