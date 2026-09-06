using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
// Moves the player forward at a fixed speed for scripted sequences.
public class ScriptedForwardWalker : MonoBehaviour
{
  static readonly int XPosParam = Animator.StringToHash("xPos");
  static readonly int ZPosParam = Animator.StringToHash("zPos");

  [SerializeField, Min(0f)] float walkSpeed = 1.5f;

  CharacterController controller;
  Animator animator;
  Vector3 normalizedMoveDirection;
  Vector3 moveDirection = Vector3.back;
  float verticalVelocity;
  float gravity = 20f;
  float animationDampTime = 0.1f;
  bool isWalking;

  public float CurrentSpeed => isWalking ? walkSpeed : 0f;

  // Caches controller and animator components and normalizes the travel direction.
  void Awake()
  {
    controller = GetComponent<CharacterController>();
    animator = GetComponent<Animator>();
    normalizedMoveDirection = moveDirection.sqrMagnitude > 0.0001f ? moveDirection.normalized : Vector3.back;
  }

  // Resets vertical velocity when the scripted walker becomes active.
  void OnEnable()
  {
    verticalVelocity = 0f;
  }

  // Enables forward movement for the scripted sequence.
  public void StartWalking()
  {
    isWalking = true;
  }

  // Applies gravity, moves the character, and updates walking animation parameters.
  void Update()
  {
    if (controller.isGrounded && verticalVelocity < 0f)
    {
      verticalVelocity = -1f;
    }
    else
    {
      verticalVelocity -= gravity * Time.deltaTime;
    }

    Vector3 motion = (normalizedMoveDirection * CurrentSpeed) + (Vector3.up * verticalVelocity);
    controller.Move(motion * Time.deltaTime);

    animator.SetFloat(XPosParam, 0f, animationDampTime, Time.deltaTime);
    animator.SetFloat(ZPosParam, isWalking ? 1f : 0f, animationDampTime, Time.deltaTime);
  }
}
