using UnityEngine;
using UnityEngine.AI;







public class RoamState : IEnemyState
{
  EnemyController enemy;
  float waitTimer;

  public void Enter(EnemyController enemy)
  {
    this.enemy = enemy;
    enemy.Detection.OnPlayerSpotted += HandlePlayerSpotted;
    enemy.Health.OnDamaged += HandleDamaged;
    waitTimer = 0f;
    PickNewDestination();
  }

  public void Tick(EnemyController enemy)
  {
    if (!enemy.Agent.isActiveAndEnabled || !enemy.Agent.isOnNavMesh)
    {
      return;
    }

    if (enemy.Agent.pathPending)
    {
      return;
    }

    if (enemy.Agent.remainingDistance > enemy.Agent.stoppingDistance)
    {
      return;
    }

    waitTimer += Time.deltaTime;
    if (waitTimer >= enemy.RoamWaitTime)
    {
      waitTimer = 0f;
      PickNewDestination();
    }
  }

  public void Exit(EnemyController enemy)
  {
    enemy.Detection.OnPlayerSpotted -= HandlePlayerSpotted;
    enemy.Health.OnDamaged -= HandleDamaged;
  }

  void PickNewDestination()
  {
    if (!enemy.Agent.isActiveAndEnabled || !enemy.Agent.isOnNavMesh)
    {
      return;
    }

    Vector3 candidate = enemy.transform.position + (Random.insideUnitSphere * enemy.RoamRadius);
    if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, enemy.RoamRadius, NavMesh.AllAreas))
    {
      enemy.Agent.SetDestination(hit.position);
    }
  }

  void HandlePlayerSpotted()
  {
    enemy.ChangeState(new PositionState());
  }

  void HandleDamaged(Vector3 hitPoint)
  {
    enemy.ChangeState(new PositionState());
  }
}
