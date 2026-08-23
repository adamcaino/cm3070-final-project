using System.Collections.Generic;
using UnityEngine;

public class SpinAttack : MeleeHitboxAttack
{
  [SerializeField] AudioClip spinSfx;

  [Tooltip("Seconds between damage ticks for a target still standing in the spin's hitbox.")]
  [SerializeField, Min(0f)] float tickInterval = 0.5f;

  readonly Dictionary<IDamageable, float> nextTickTime = new Dictionary<IDamageable, float>();
  bool isHitboxActive;

  protected override void OnExecute(EnemyController enemy)
  {
    PlayAttackAnimation(enemy);
  }

  void OnTriggerEnter(Collider other) => TryTick(other);
  void OnTriggerStay(Collider other) => TryTick(other);

  public void PlaySpinSfx()
  {
    if (spinSfx != null)
    {
      GetComponent<AudioSource>().PlayOneShot(spinSfx);
    }
  }

  void TryTick(Collider other)
  {
    if (!isHitboxActive) return;

    IDamageable damageable = other.GetComponentInParent<IDamageable>();
    if (damageable == null) return;

    if (nextTickTime.TryGetValue(damageable, out float tickTime) && Time.time < tickTime) return;

    nextTickTime[damageable] = Time.time + tickInterval;
    DealDamage(damageable, other);
  }

  public void EnableSpinHitbox()
  {
    nextTickTime.Clear();
    isHitboxActive = true;
    ActivateHitbox();
  }

  public void DisableSpinHitbox()
  {
    isHitboxActive = false;
    DeactivateHitbox();
  }
}
