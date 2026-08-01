using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Reads the Move action and drives a CharacterController relative to the active camera's facing,
/// so "forward" always means "away from camera" regardless of which way the player model is turned.
/// The body's yaw is slaved to the camera's yaw every frame (not just while moving) - the camera is
/// the sole source of facing (see PlayerCameraOrbit), so WASD becomes a true strafe/backpedal input
/// relative to a fixed forward instead of turning the character itself.
/// </summary>
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

  CharacterController controller;
  Transform cameraTransform;
  Vector2 moveInput;
  Vector2 effectiveMoveInput;
  float verticalVelocity;

  public float CurrentSpeed { get; private set; }
  public Vector3 PlanarVelocity { get; private set; }
  public float MoveSpeed => moveSpeed;
  public float LocalStrafe => effectiveMoveInput.x;
  public float LocalForward => effectiveMoveInput.y;

  void Awake()
  {
    controller = GetComponent<CharacterController>();
    cameraTransform = Camera.main != null ? Camera.main.transform : null;
  }

  void OnEnable()
  {
    moveAction?.action.Enable();
  }

  void OnDisable()
  {
    moveAction?.action.Disable();
  }

  void Update()
  {
    moveInput = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
    effectiveMoveInput = new Vector2(
      moveInput.x,
      moveInput.y < 0f ? moveInput.y * backwardSpeedMultiplier : moveInput.y);

    Vector3 moveDirection = CalculateMoveDirection();
    ApplyGravity();

    Vector3 motion = (moveDirection * moveSpeed) + (Vector3.up * verticalVelocity);
    controller.Move(motion * Time.deltaTime);

    PlanarVelocity = new Vector3(motion.x, 0f, motion.z);
    CurrentSpeed = PlanarVelocity.magnitude;

    RotateTowardsCamera();
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
