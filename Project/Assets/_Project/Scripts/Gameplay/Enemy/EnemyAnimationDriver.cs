using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(EnemyController))]
public class EnemyAnimationDriver : MonoBehaviour
{
  static readonly int IsMovingParam = Animator.StringToHash("isMoving");
  static readonly int GetHitParam = Animator.StringToHash("getHit");

  [SerializeField, Range(0f, 1f)] float getHitChance = 0.15f;
  [SerializeField] ParticleSystem phase2Glow;

  Animator animator;
  EnemyController enemy;
  Health health;
  BossPhaseController bossPhase;

  void Awake()
  {
    animator = GetComponent<Animator>();
    enemy = GetComponent<EnemyController>();
    health = GetComponent<Health>();
    bossPhase = GetComponent<BossPhaseController>();

    if (phase2Glow != null) phase2Glow.Stop();
  }

  void OnEnable()
  {
    health.OnDamaged += HandleDamaged;

    // Only subscribe to the boss phase change event if this enemy has a boss phase controller component
    if (bossPhase != null) bossPhase.OnStageChanged += HandleStageChanged;
  }

  void OnDisable()
  {
    health.OnDamaged -= HandleDamaged;

    // Only unsubscribe to the boss phase change event if this enemy has a boss phase controller component
    if (bossPhase != null) bossPhase.OnStageChanged -= HandleStageChanged;
  }

  void Update()
  {
    animator.SetBool(IsMovingParam, enemy.IsMoving);
  }

  void HandleDamaged(Vector3 hitPoint)
  {
    // If this hit kills the boss, stop the phase 2 glow and exit early.
    if (health.CurrentHealth == 0)
    {
      if (phase2Glow != null) phase2Glow.gameObject.SetActive(false);

      return;
    }

    // Quick "critical hit" calculation based on getHitChance
    if (Random.value > getHitChance) return;

    animator.SetTrigger(GetHitParam);
  }

  // Only ever called when the boss changes stage (i.e. Stage 1 > Stage 2)
  void HandleStageChanged(int stage)
  {
    animator.SetTrigger(GetHitParam);

    // Play the phase 2 glow effect when the boss enters stage 2.
    if (phase2Glow != null) phase2Glow.Play();
  }
}
