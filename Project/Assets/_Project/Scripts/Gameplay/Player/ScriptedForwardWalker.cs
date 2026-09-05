using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
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

  void Awake()
  {
    controller = GetComponent<CharacterController>();
    animator = GetComponent<Animator>();
    normalizedMoveDirection = moveDirection.sqrMagnitude > 0.0001f ? moveDirection.normalized : Vector3.back;
  }

  void OnEnable()
  {
    verticalVelocity = 0f;
  }

  public void StartWalking()
  {
    isWalking = true;
  }

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
