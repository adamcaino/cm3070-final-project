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

    enemy.FaceTowards(enemy.Player.position);

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

    // Attack as soon as an attack is valid for distance/angle; waiting for near-zero
    // velocity can stall melee enemies that keep tiny navmesh drift.
    float distance = Vector3.Distance(enemy.transform.position, enemy.Player.position);

    Vector3 toPlayer = enemy.Player.position - enemy.transform.position;
    toPlayer.y = 0f;
    float angle = toPlayer.sqrMagnitude > 0.0001f ? Vector3.Angle(enemy.transform.forward, toPlayer) : 0f;

    IAttack[] executableAttacks = enemy.Attacks
      .Where(a => a.CanExecute)
      .OrderByDescending(a => a.Priority)
      .ToArray();

    if (executableAttacks.Length == 0)
    {
      return;
    }

    IAttack matchingAttack = executableAttacks
      .Where(a => distance >= a.MinRange && distance <= a.MaxRange
        && angle >= a.MinAngle && angle <= a.MaxAngle)
      .FirstOrDefault(a => Random.value <= a.SelectionChance);

    // If chance rolls skip every candidate this frame, still pick the top valid attack
    // so phase 1 cannot stall forever behind unlucky RNG.
    if (matchingAttack == null)
    {
      matchingAttack = executableAttacks.FirstOrDefault(a =>
        distance >= a.MinRange && distance <= a.MaxRange
        && angle >= a.MinAngle && angle <= a.MaxAngle);
    }

    if (matchingAttack != null)
    {
      enemy.Agent.ResetPath();
      enemy.ChangeState(new AttackState(matchingAttack));
      return;
    }

    IAttack targetAttack = executableAttacks[0];

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
