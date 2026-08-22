using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Default idle behaviour - wander to random points within RoamRadius, pausing RoamWaitTime between legs.
/// Hands off to PositionState the instant EnemyDetection spots the player, or the instant the enemy takes
/// damage - a hit from outside its vision cone (a sneak attack) should still turn it on the attacker
/// rather than leaving it obliviously wandering.
/// </summary>
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
