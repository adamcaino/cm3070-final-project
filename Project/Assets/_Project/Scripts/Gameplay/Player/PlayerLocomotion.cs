using UnityEngine;
using UnityEngine.InputSystem;








[RequireComponent(typeof(CharacterController))]
public class PlayerLocomotion : MonoBehaviour
{
  [Header("Input")]
  [SerializeField] InputActionReference moveAction;

  [Header("Movement")]
  [SerializeField, Min(0f)] float moveSpeed = 5f;
  [SerializeField, Range(0f, 1f)] float backwardSpeedMultiplier = 0.5f;
  [SerializeField, Min(0f)] float rotationSpeed = 720f;
  [SerializeField, Min(0f)] float gravity = 20f;

  [Header("Target Lock")]
  [Tooltip("How long the body takes to turn onto a newly locked/cycled target, instead of snapping - 0 is an instant snap. Ongoing tracking of a single locked target's movement isn't affected, only the moment of switching.")]
  [SerializeField, Min(0f)] float lockedTurnDuration = 0.5f;

  CharacterController controller;
  Transform cameraTransform;
  TargetLockController targetLock;
  Vector2 moveInput;
  Vector2 effectiveMoveInput;
  float verticalVelocity;
  float lockedTurnTimer;
  Quaternion lockedTurnStartRotation;

  public float CurrentSpeed { get; private set; }
  public Vector3 PlanarVelocity { get; private set; }
  public float MoveSpeed => moveSpeed;
  public float LocalStrafe => effectiveMoveInput.x;
  public float LocalForward => effectiveMoveInput.y;

  void Awake()
  {
    controller = GetComponent<CharacterController>();
    cameraTransform = Camera.main != null ? Camera.main.transform : null;
    targetLock = GetComponent<TargetLockController>();
  }

  void OnEnable()
  {
    moveAction?.action.Enable();

    if (targetLock != null)
    {
      targetLock.OnLockOn += HandleLockOn;
    }
  }

  void OnDisable()
  {
    moveAction?.action.Disable();

    if (targetLock != null)
    {
      targetLock.OnLockOn -= HandleLockOn;
    }
  }

  void HandleLockOn(Transform target)
  {
    lockedTurnTimer = 0f;
    lockedTurnStartRotation = transform.rotation;
  }

  void Update()
  {
    moveInput = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
    effectiveMoveInput = new Vector2(
      moveInput.x,
      moveInput.y < 0f ? moveInput.y * backwardSpeedMultiplier : moveInput.y);

    Transform lockedTarget = targetLock != null && targetLock.IsLocked ? targetLock.LockedTarget : null;

    Vector3 moveDirection = lockedTarget != null ? CalculateLockedMoveDirection(lockedTarget) : CalculateMoveDirection();
    ApplyGravity();

    Vector3 motion = (moveDirection * moveSpeed) + (Vector3.up * verticalVelocity);
    controller.Move(motion * Time.deltaTime);

    PlanarVelocity = new Vector3(motion.x, 0f, motion.z);
    CurrentSpeed = PlanarVelocity.magnitude;

    if (lockedTarget != null)
    {
      RotateTowardsTarget(lockedTarget);
    }
    else
    {
      RotateTowardsCamera();
    }
  }

  Vector3 CalculateMoveDirection()
  {
    if (effectiveMoveInput.sqrMagnitude < 0.0001f)
    {
      return Vector3.zero;
    }

    Transform reference = cameraTransform != null ? cameraTransform : Camera.main != null ? Camera.main.transform : null;
    Vector3 forward;
    Vector3 right;

    if (reference != null)
    {
      forward = Vector3.ProjectOnPlane(reference.forward, Vector3.up).normalized;
      right = Vector3.ProjectOnPlane(reference.right, Vector3.up).normalized;
    }
    else
    {
      forward = Vector3.forward;
      right = Vector3.right;
    }

    Vector3 direction = (forward * effectiveMoveInput.y) + (right * effectiveMoveInput.x);
    return direction.sqrMagnitude > 1f ? direction.normalized : direction;
  }

  Vector3 CalculateLockedMoveDirection(Transform target)
  {
    if (effectiveMoveInput.sqrMagnitude < 0.0001f)
    {
      return Vector3.zero;
    }

    Vector3 forward = Vector3.ProjectOnPlane(target.position - transform.position, Vector3.up).normalized;
    if (forward.sqrMagnitude < 0.0001f)
    {
      return Vector3.zero;
    }

    Vector3 right = Vector3.Cross(Vector3.up, forward);
    Vector3 direction = (forward * effectiveMoveInput.y) + (right * effectiveMoveInput.x);
    return direction.sqrMagnitude > 1f ? direction.normalized : direction;
  }

  void RotateTowardsTarget(Transform target)
  {
    Vector3 forward = Vector3.ProjectOnPlane(target.position - transform.position, Vector3.up);
    if (forward.sqrMagnitude < 0.0001f)
    {
      return;
    }

    Quaternion targetRotation = Quaternion.LookRotation(forward, Vector3.up);

    if (lockedTurnTimer < lockedTurnDuration)
    {
      lockedTurnTimer += Time.deltaTime;
      float t = lockedTurnDuration > 0f ? Mathf.Clamp01(lockedTurnTimer / lockedTurnDuration) : 1f;
      transform.rotation = Quaternion.Slerp(lockedTurnStartRotation, targetRotation, t);
    }
    else
    {
      transform.rotation = targetRotation;
    }
  }

  void RotateTowardsCamera()
  {
    Transform reference = cameraTransform != null ? cameraTransform : Camera.main != null ? Camera.main.transform : null;
    if (reference == null)
    {
      return;
    }

    Vector3 forward = Vector3.ProjectOnPlane(reference.forward, Vector3.up);
    if (forward.sqrMagnitude < 0.0001f)
    {
      return;
    }

    Quaternion targetRotation = Quaternion.LookRotation(forward, Vector3.up);
    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
  }

  void ApplyGravity()
  {
    if (controller.isGrounded && verticalVelocity < 0f)
    {
      verticalVelocity = -1f;
      return;
    }

    verticalVelocity -= gravity * Time.deltaTime;
  }
}
