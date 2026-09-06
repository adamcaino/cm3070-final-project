using System.Collections;
using UnityEngine;
using UnityEngine.AI;



// Disables enemy interaction and settles its navigation agent after death.
public class DeadState : IEnemyState
{
  static readonly int DeadParam = Animator.StringToHash("dead");

  // Stops movement, disables colliders, plays the death animation, and settles the agent.
  public void Enter(EnemyController enemy)
  {
    // A flying/hovering agent (baseOffset > 0) can die mid-transition, off the navmesh it's nominally
    // tracking - isStopped throws if set on an agent that isn't currently active and placed on a mesh.
    if (enemy.Agent.isActiveAndEnabled && enemy.Agent.isOnNavMesh)
    {
      enemy.Agent.isStopped = true;
    }

    foreach (Collider enemyCollider in enemy.GetComponents<Collider>())
    {
      enemyCollider.enabled = false;
    }

    bool diedFrozen = enemy.StatusEffects != null && enemy.StatusEffects.IsFrozen;

    if (!diedFrozen && enemy.Animator != null)
    {
      enemy.Animator.SetBool(DeadParam, true);
    }

    if (enemy.Agent.baseOffset != 0f && enemy.DeathBaseOffsetSettleDuration > 0f)
    {
      enemy.StartCoroutine(SettleBaseOffsetRoutine(enemy));
    }
    else
    {
      enemy.Agent.enabled = false;
    }
  }

  // Keeps the enemy in its terminal state.
  public void Tick(EnemyController enemy) { }

  // Provides the state-machine exit hook; dead enemies have no active state to restore.
  public void Exit(EnemyController enemy) { }

  // Gradually lowers a hovering agent before disabling its navigation component.
  IEnumerator SettleBaseOffsetRoutine(EnemyController enemy)
  {
    NavMeshAgent agent = enemy.Agent;
    float startOffset = agent.baseOffset;
    float duration = enemy.DeathBaseOffsetSettleDuration;
    float elapsed = 0f;

    while (elapsed < duration)
    {
      elapsed += Time.deltaTime;
      agent.baseOffset = Mathf.Lerp(startOffset, 0f, elapsed / duration);
      yield return null;
    }

    agent.baseOffset = 0f;
    agent.enabled = false;
  }
}
