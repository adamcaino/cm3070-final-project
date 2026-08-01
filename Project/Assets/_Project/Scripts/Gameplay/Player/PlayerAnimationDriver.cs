using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationDriver : MonoBehaviour
{
  static readonly int XPosParam = Animator.StringToHash("xPos");
  static readonly int ZPosParam = Animator.StringToHash("zPos");

  [SerializeField, Min(0f)] float dampTime = 0.1f;

  Animator animator;
  PlayerLocomotion locomotion;

  void Awake()
  {
    animator = GetComponent<Animator>();
    locomotion = GetComponent<PlayerLocomotion>();
  }

  void Update()
  {
    if (locomotion == null)
    {
      return;
    }

    animator.SetFloat(XPosParam, locomotion.LocalStrafe, dampTime, Time.deltaTime);
    animator.SetFloat(ZPosParam, locomotion.LocalForward, dampTime, Time.deltaTime);
  }
}
