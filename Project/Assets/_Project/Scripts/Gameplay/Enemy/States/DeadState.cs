using UnityEngine;

/// <summary>
/// Terminal state - reached directly from EnemyController on Health.OnDied regardless of which state was
/// active. Stops the agent and disables colliders so a dead enemy no longer blocks pathing or takes hits;
/// leaves the GameObject itself alone (destroy/pool/loot-drop is EnemyLootDrop's job via the same event).
/// </summary>
public class DeadState : IEnemyState
{
  static readonly int DeadParam = Animator.StringToHash("dead");

  public void Enter(EnemyController enemy)
  {
    enemy.Agent.isStopped = true;
    enemy.Agent.enabled = false;

    foreach (Collider enemyCollider in enemy.GetComponents<Collider>())
    {
      enemyCollider.enabled = false;
    }

    if (enemy.Animator != null)
    {
      enemy.Animator.SetBool(DeadParam, true);
    }
  }

  public void Tick(EnemyController enemy) { }

  public void Exit(EnemyController enemy) { }
}
