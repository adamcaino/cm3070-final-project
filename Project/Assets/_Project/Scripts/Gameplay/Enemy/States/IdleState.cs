public class IdleState : IEnemyState
{
  public void Enter(EnemyController enemy)
  {
    if (enemy.Agent.isActiveAndEnabled && enemy.Agent.isOnNavMesh)
    {
      enemy.Agent.isStopped = true;
      enemy.Agent.ResetPath();
    }
  }

  public void Tick(EnemyController enemy) { }

  public void Exit(EnemyController enemy)
  {
    if (enemy.Agent.isActiveAndEnabled && enemy.Agent.isOnNavMesh)
    {
      enemy.Agent.isStopped = false;
    }
  }
}
