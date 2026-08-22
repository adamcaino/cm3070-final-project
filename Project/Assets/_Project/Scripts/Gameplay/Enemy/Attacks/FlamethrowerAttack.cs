using UnityEngine;

[RequireComponent(typeof(Health))]
public class FlamethrowerAttack : AttackBase
{
  [SerializeField] GameObject flamethrowerVFX;
  [Tooltip("How long already-emitted particles are left to fade out after Stop() before the VFX GameObject is deactivated.")]
  [SerializeField, Min(0f)] float vfxLingerTime = 1f;

  FlameHitbox hitbox;
  ParticleSystem flameParticles;
  Health health;

  void Awake()
  {
    hitbox = flamethrowerVFX.GetComponent<FlameHitbox>();
    flameParticles = flamethrowerVFX.GetComponent<ParticleSystem>();
    health = GetComponent<Health>();
  }

  void OnEnable()
  {
    health.OnDied += HandleDied;
  }

  void OnDisable()
  {
    health.OnDied -= HandleDied;
  }

  void Start()
  {
    flamethrowerVFX.SetActive(false);
  }

  void HandleDied() => DisableFlame();

  protected override void OnExecute(EnemyController enemy)
  {
    PlayAttackAnimation(enemy);
    hitbox.Configure(Damage, enemy.gameObject);
  }

  public void EnableFlame()
  {
    CancelInvoke(nameof(DeactivateFlameVFX));
    flamethrowerVFX.SetActive(true);
    hitbox.Enable();
  }

  public void DisableFlame()
  {
    flameParticles.Stop();
    hitbox.Disable();

    CancelInvoke(nameof(DeactivateFlameVFX));
    Invoke(nameof(DeactivateFlameVFX), vfxLingerTime);
    RaiseAttackComplete();
  }

  // Deactivates the VFX GameObject once already-emitted particles have had time to fade out naturally.
  void DeactivateFlameVFX()
  {
    flamethrowerVFX.SetActive(false);
  }
}
