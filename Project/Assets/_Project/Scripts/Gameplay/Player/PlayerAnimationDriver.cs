using UnityEngine;

[RequireComponent(typeof(Animator))]
// Drives player animation movement parameters from the locomotion component.
public class PlayerAnimationDriver : MonoBehaviour
{
  static readonly int XPosParam = Animator.StringToHash("xPos");
  static readonly int ZPosParam = Animator.StringToHash("zPos");

  [SerializeField, Min(0f)] float dampTime = 0.1f;

  Animator animator;
  PlayerLocomotion locomotion;

  // Caches the animator and locomotion components.
  void Awake()
  {
    animator = GetComponent<Animator>();
    locomotion = GetComponent<PlayerLocomotion>();
  }

  // Updates damped local movement values on the animator each frame.
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
