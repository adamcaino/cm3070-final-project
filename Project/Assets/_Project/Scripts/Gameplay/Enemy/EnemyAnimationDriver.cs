using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(EnemyController))]
public class EnemyAnimationDriver : MonoBehaviour
{
  static readonly int IsMovingParam = Animator.StringToHash("isMoving");
  static readonly int GetHitParam = Animator.StringToHash("getHit");

  [SerializeField, Range(0f, 1f)] float getHitChance = 0.15f;

  Animator animator;
  EnemyController enemy;
  Health health;

  void Awake()
  {
    animator = GetComponent<Animator>();
    enemy = GetComponent<EnemyController>();
    health = GetComponent<Health>();
  }

  void OnEnable()
  {
    health.OnDamaged += HandleDamaged;
  }

  void OnDisable()
  {
    health.OnDamaged -= HandleDamaged;
  }

  void Update()
  {
    animator.SetBool(IsMovingParam, enemy.IsMoving);
  }

  void HandleDamaged(Vector3 hitPoint)
  {
    // If the enemy is dead, don't play the get-hit animation - the death animation will be played instead.
    if (health.IsDead) return;

    // Only occasionally flinch on hit - playing it every time let players stun-lock enemies by timing attacks.
    if (Random.value > getHitChance) return;

    animator.SetTrigger(GetHitParam);
  }
}
