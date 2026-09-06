using UnityEngine;
using UnityEngine.AI;







// Moves the enemy between random navigation points until the player is detected or it is hit.
public class RoamState : IEnemyState
{
  EnemyController enemy;
  float waitTimer;

  // Stores the enemy reference, subscribes to awareness events, and selects a destination.
  public void Enter(EnemyController enemy)
  {
    this.enemy = enemy;
    enemy.Detection.OnPlayerSpotted += HandlePlayerSpotted;
    enemy.Health.OnDamaged += HandleDamaged;
    waitTimer = 0f;
    PickNewDestination();
  }

  // Waits at each destination before selecting another valid navigation point.
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

  // Removes the player-spotted and damage event subscriptions.
  public void Exit(EnemyController enemy)
  {
    enemy.Detection.OnPlayerSpotted -= HandlePlayerSpotted;
    enemy.Health.OnDamaged -= HandleDamaged;
  }

  // Samples a random point around the enemy and assigns it as the navigation destination.
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

  // Switches to positioning when the player enters the detection range.
  void HandlePlayerSpotted()
  {
    enemy.ChangeState(new PositionState());
  }

  // Switches to positioning when the enemy takes damage.
  void HandleDamaged(Vector3 hitPoint)
  {
    enemy.ChangeState(new PositionState());
  }
}
