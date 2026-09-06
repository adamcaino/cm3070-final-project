using UnityEngine;

[RequireComponent(typeof(Health))]
// Controls a particle flamethrower and its damage hitbox as an enemy attack.
public class FlamethrowerAttack : AttackBase
{
  [SerializeField] GameObject flamethrowerVFX;
  [Tooltip("How long already-emitted particles are left to fade out after Stop() before the VFX GameObject is deactivated.")]
  [SerializeField, Min(0f)] float vfxLingerTime = 1f;

  FlameHitbox hitbox;
  ParticleSystem flameParticles;
  Health health;

  // Caches the flame components and subscribes their owner to the required health source.
  protected override void Awake()
  {
    base.Awake();

    hitbox = flamethrowerVFX.GetComponent<FlameHitbox>();
    flameParticles = flamethrowerVFX.GetComponent<ParticleSystem>();
    health = GetComponent<Health>();
  }

  // Subscribes to the owner's death event.
  void OnEnable()
  {
    health.OnDied += HandleDied;
  }

  // Removes the owner's death event subscription.
  void OnDisable()
  {
    health.OnDied -= HandleDied;
  }

  // Hides the flame effect before the first attack.
  void Start()
  {
    flamethrowerVFX.SetActive(false);
  }

  // Stops the flame when its owner dies.
  void HandleDied() => DisableFlame();

  // Stops the flame when the attack is interrupted by the state machine.
  public override void Interrupt() => DisableFlame();

  // Plays the attack animation and configures the hitbox damage source.
  protected override void OnExecute(EnemyController enemy)
  {
    PlayAttackAnimation(enemy);
    hitbox.Configure(Damage, enemy.gameObject);
  }

  // Activates the flame visual and enables particle damage.
  public void EnableFlame()
  {
    CancelInvoke(nameof(DeactivateFlameVFX));
    flamethrowerVFX.SetActive(true);
    hitbox.Enable();
  }

  // Stops particle emission, disables damage, and completes the attack after cleanup begins.
  public void DisableFlame()
  {
    flameParticles.Stop();
    hitbox.Disable();

    CancelInvoke(nameof(DeactivateFlameVFX));
    Invoke(nameof(DeactivateFlameVFX), vfxLingerTime);
    RaiseAttackComplete();
  }


  // Deactivates the flame object after its lingering particles have faded.
  void DeactivateFlameVFX()
  {
    flamethrowerVFX.SetActive(false);
  }
}
