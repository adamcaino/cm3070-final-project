using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
// Applies periodic damage when the particle system overlaps the player.
public class FlameHitbox : MonoBehaviour
{
  const string PLAYERTAG = "Player";

  [SerializeField, Min(0f)] float tickInterval = 0.25f;

  ParticleSystem particles;
  readonly List<ParticleSystem.Particle> triggerBuffer = new List<ParticleSystem.Particle>();

  Collider playerCollider;
  IDamageable playerDamageable;
  int damage;
  GameObject source;
  float nextTickTime;

  // Configures particle trigger callbacks and attempts to find the player collider.
  void Awake()
  {
    particles = GetComponent<ParticleSystem>();

    ParticleSystem.TriggerModule trigger = particles.trigger;
    trigger.enabled = true;
    trigger.inside = ParticleSystemOverlapAction.Callback;

    TryResolvePlayer();
  }

  // Stores the damage and source used for subsequent particle hits.
  public void Configure(int attackDamage, GameObject attackSource)
  {
    damage = attackDamage;
    source = attackSource;

    TryResolvePlayer();
  }

  // Allows the next particle overlap to apply damage immediately.
  public void Enable()
  {
    nextTickTime = 0f;
  }

  // Provides the attack lifecycle hook for disabling the flame hitbox.
  public void Disable() { }

  // Applies damage when particles overlap the player and the tick interval has elapsed.
  void OnParticleTrigger()
  {
    if (playerDamageable == null || Time.time < nextTickTime) return;

    int insideCount = particles.GetTriggerParticles(ParticleSystemTriggerEventType.Inside, triggerBuffer);
    if (insideCount == 0) return;

    nextTickTime = Time.time + tickInterval;

    Vector3 hitPoint = playerCollider != null ? playerCollider.ClosestPointOnBounds(transform.position) : transform.position;
    playerDamageable.TakeDamage(damage, source, hitPoint);
  }

  // Finds the player damage contract and assigns its collider to the particle trigger module.
  void TryResolvePlayer()
  {
    if (playerDamageable != null) return;

    GameObject playerObject = GameObject.FindGameObjectWithTag(PLAYERTAG);
    if (playerObject == null) return;

    playerCollider = playerObject.GetComponent<CapsuleCollider>();
    playerDamageable = playerObject.GetComponent<IDamageable>();

    if (playerCollider != null)
    {
      ParticleSystem.TriggerModule trigger = particles.trigger;
      trigger.SetCollider(0, playerCollider);
    }
  }
}
