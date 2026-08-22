using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Applies Burn (periodic Health damage) and Freeze (NavMeshAgent speed to zero) over time. Only one
/// affliction is active at a time - a new application replaces whatever's running rather than
/// stacking, restoring Freeze's cached speed first so it can't stomp the agent's real base speed.
/// </summary>
[RequireComponent(typeof(Health))]
public class StatusEffectReceiver : MonoBehaviour, IAfflictable
{
  const float BurnTickInterval = 1f;

  Health health;
  NavMeshAgent agent;
  Coroutine activeEffect;
  float cachedAgentSpeed;
  bool isFrozen;

  void Awake()
  {
    health = GetComponent<Health>();
    agent = GetComponent<NavMeshAgent>();
  }

  public void ApplyAffliction(AfflictionType type, float duration, int magnitude, GameObject source)
  {
    if (activeEffect != null)
    {
      StopCoroutine(activeEffect);
      activeEffect = null;
    }
    EndFreeze();

    switch (type)
    {
      case AfflictionType.Burn:
        activeEffect = StartCoroutine(BurnRoutine(duration, magnitude, source));
        break;
      case AfflictionType.Freeze:
        activeEffect = StartCoroutine(FreezeRoutine(duration));
        break;
    }
  }

  IEnumerator BurnRoutine(float duration, int magnitude, GameObject source)
  {
    float elapsed = 0f;
    while (elapsed < duration)
    {
      yield return new WaitForSeconds(BurnTickInterval);
      elapsed += BurnTickInterval;

      if (health.IsDead)
      {
        yield break;
      }

      health.TakeDamage(magnitude, source, health.transform.position);
    }

    activeEffect = null;
  }

  IEnumerator FreezeRoutine(float duration)
  {
    if (agent == null)
    {
      activeEffect = null;
      yield break;
    }

    cachedAgentSpeed = agent.speed;
    agent.speed = 0f;
    isFrozen = true;

    yield return new WaitForSeconds(duration);

    EndFreeze();
    activeEffect = null;
  }

  void EndFreeze()
  {
    if (!isFrozen)
    {
      return;
    }

    agent.speed = cachedAgentSpeed;
    isFrozen = false;
  }
}
