using System.Collections.Generic;
using UnityEngine;

// Applies repeated damage ticks to targets inside the active spinning hitbox.
public class SpinAttack : MeleeHitboxAttack
{
  [SerializeField] AudioClip spinSfx;

  [Tooltip("Seconds between damage ticks for a target still standing in the spin's hitbox.")]
  [SerializeField, Min(0f)] float tickInterval = 0.5f;

  readonly Dictionary<IDamageable, float> nextTickTime = new Dictionary<IDamageable, float>();
  bool isHitboxActive;

  // Starts the configured spin animation.
  protected override void OnExecute(EnemyController enemy)
  {
    PlayAttackAnimation(enemy);
  }

  // Attempts the first damage tick when a target enters the spin hitbox.
  void OnTriggerEnter(Collider other) => TryTick(other);

  // Attempts subsequent damage ticks while a target remains in the spin hitbox.
  void OnTriggerStay(Collider other) => TryTick(other);

  // Plays the configured spin sound through the attached audio source.
  public void PlaySpinSfx()
  {
    if (spinSfx != null)
    {
      GetComponent<AudioSource>().PlayOneShot(spinSfx);
    }
  }

  // Applies damage when the target's per-target tick interval has elapsed.
  void TryTick(Collider other)
  {
    if (!isHitboxActive) return;

    IDamageable damageable = other.GetComponentInParent<IDamageable>();
    if (damageable == null) return;

    if (nextTickTime.TryGetValue(damageable, out float tickTime) && Time.time < tickTime) return;

    nextTickTime[damageable] = Time.time + tickInterval;
    DealDamage(damageable, other);
  }

  // Clears target timers and activates the spin hitbox during execution.
  public void EnableSpinHitbox()
  {
    if (!IsExecuting) return;

    nextTickTime.Clear();
    isHitboxActive = true;
    ActivateHitbox();
  }

  // Deactivates the spin hitbox and completes the attack.
  public void DisableSpinHitbox()
  {
    if (!isHitboxActive) return;

    isHitboxActive = false;
    DeactivateHitbox();
  }
}
