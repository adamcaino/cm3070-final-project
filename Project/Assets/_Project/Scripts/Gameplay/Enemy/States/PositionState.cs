using System.Linq;
using UnityEngine;













public class PositionState : IEnemyState
{
  EnemyController enemy;
  float memoryTimer;

  public void Enter(EnemyController enemy)
  {
    this.enemy = enemy;
    if (enemy.Agent.isActiveAndEnabled && enemy.Agent.isOnNavMesh)
    {
      enemy.Agent.isStopped = false;
    }
    enemy.Agent.updateRotation = false;
    memoryTimer = enemy.AggroMemoryDuration;
  }

  public void Tick(EnemyController enemy)
  {
    if (enemy.Player == null || enemy.Attacks.Count == 0)
    {
      return;
    }

    if (!enemy.Agent.isActiveAndEnabled || !enemy.Agent.isOnNavMesh)
    {
      return;
    }

    if (enemy.Detection.CanSeePlayer)
    {
      memoryTimer = enemy.AggroMemoryDuration;
    }
    else
    {
      memoryTimer -= Time.deltaTime;
      if (memoryTimer <= 0f)
      {
        enemy.ChangeState(new RoamState());
        return;
      }
    }

    enemy.FaceTowards(enemy.Player.position);

    float distance = Vector3.Distance(enemy.transform.position, enemy.Player.position);

    Vector3 toPlayer = enemy.Player.position - enemy.transform.position;
    toPlayer.y = 0f;
    float angle = toPlayer.sqrMagnitude > 0.0001f ? Vector3.Angle(enemy.transform.forward, toPlayer) : 0f;



    if (!enemy.IsMoving)
    {




      IAttack matchingAttack = enemy.Attacks
        .Where(a => a.CanExecute && distance >= a.MinRange && distance <= a.MaxRange
          && angle >= a.MinAngle && angle <= a.MaxAngle)
        .OrderByDescending(a => a.Priority)
        .FirstOrDefault(a => Random.value <= a.SelectionChance);

      if (matchingAttack != null)
      {
        enemy.ChangeState(new AttackState(matchingAttack));
        return;
      }
    }



    IAttack targetAttack = enemy.Attacks.OrderByDescending(a => a.Priority).First();

    if (distance > targetAttack.MaxRange)
    {
      enemy.Agent.SetDestination(enemy.Player.position);
    }
    else if (distance < targetAttack.MinRange)
    {
      Vector3 awayDirection = (enemy.transform.position - enemy.Player.position).normalized;
      Vector3 retreatPoint = enemy.Player.position + (awayDirection * targetAttack.MinRange);
      enemy.Agent.SetDestination(retreatPoint);
    }
    else
    {
      enemy.Agent.ResetPath();
    }
  }

  public void Exit(EnemyController enemy)
  {
    enemy.Agent.updateRotation = true;
  }
}
