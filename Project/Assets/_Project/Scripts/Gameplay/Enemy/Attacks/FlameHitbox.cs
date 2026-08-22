using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
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

  void Awake()
  {
    particles = GetComponent<ParticleSystem>();

    ParticleSystem.TriggerModule trigger = particles.trigger;
    trigger.enabled = true;
    trigger.inside = ParticleSystemOverlapAction.Callback;

    TryResolvePlayer();
  }

  public void Configure(int attackDamage, GameObject attackSource)
  {
    damage = attackDamage;
    source = attackSource;

    TryResolvePlayer();
  }

  public void Enable()
  {
    nextTickTime = 0f;
  }

  public void Disable() { }

  void OnParticleTrigger()
  {
    if (playerDamageable == null || Time.time < nextTickTime) return;

    int insideCount = particles.GetTriggerParticles(ParticleSystemTriggerEventType.Inside, triggerBuffer);
    if (insideCount == 0) return;

    nextTickTime = Time.time + tickInterval;

    Vector3 hitPoint = playerCollider != null ? playerCollider.ClosestPointOnBounds(transform.position) : transform.position;
    playerDamageable.TakeDamage(damage, source, hitPoint);
  }

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
