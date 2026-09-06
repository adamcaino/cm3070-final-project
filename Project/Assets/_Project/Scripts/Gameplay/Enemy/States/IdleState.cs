// Stops the enemy and clears its navigation path while it remains inactive.
public class IdleState : IEnemyState
{
  // Stops navigation and clears the current path when entering the idle state.
  public void Enter(EnemyController enemy)
  {
    if (enemy.Agent.isActiveAndEnabled && enemy.Agent.isOnNavMesh)
    {
      enemy.Agent.isStopped = true;
      enemy.Agent.ResetPath();
    }
  }

  // Keeps the enemy idle until the state machine selects another state.
  public void Tick(EnemyController enemy) { }

  // Resumes navigation when leaving the idle state.
  public void Exit(EnemyController enemy)
  {
    if (enemy.Agent.isActiveAndEnabled && enemy.Agent.isOnNavMesh)
    {
      enemy.Agent.isStopped = false;
    }
  }
}
