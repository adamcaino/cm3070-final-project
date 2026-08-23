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

    // Frozen deaths swap straight to the Enemy_Death_Frozen prefab (EnemyDeathVFX) instead of playing
    // the normal death animation - the Animator's paused (speed 0) while frozen anyway, so triggering
    // it here would just queue an animation that never plays before the object is destroyed.
    bool diedFrozen = enemy.StatusEffects != null && enemy.StatusEffects.IsFrozen;

    if (!diedFrozen && enemy.Animator != null)
    {
      enemy.Animator.SetBool(DeadParam, true);
    }
  }

  public void Tick(EnemyController enemy) { }

  public void Exit(EnemyController enemy) { }
}
