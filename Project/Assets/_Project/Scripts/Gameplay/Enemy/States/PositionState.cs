using System.Linq;
using UnityEngine;

/// <summary>
/// Generalised positioning state that reads MinRange/MaxRange off the enemy's own attacks rather than a
/// per-type ApproachState - a melee enemy closes to its attack's ~1.5 range, a ranged enemy holds at its
/// max range, a summoner holds mid-range, all through the same code path. Picked over separate per-type
/// states because this project expects more ranged variants later and the range-band read is identical
/// either way; if the enemy roster stays small this could be split back out per-type for readability.
///
/// Once aggro'd the enemy keeps chasing/facing the player's live position even without line of sight -
/// EnemyDetection only resets an AggroMemoryDuration countdown, so a player briefly ducking out of the
/// vision cone (or the enemy being mid-AttackState when that happens) doesn't instantly drop aggro. Only
/// once that countdown expires does the enemy give up and return to RoamState.
/// </summary>
public class PositionState : IEnemyState
{
  EnemyController enemy;
  float memoryTimer;

  public void Enter(EnemyController enemy)
  {
    this.enemy = enemy;
    enemy.Agent.isStopped = false;
    enemy.Agent.updateRotation = false;
    memoryTimer = enemy.AggroMemoryDuration;
  }

  public void Tick(EnemyController enemy)
  {
    if (enemy.Player == null || enemy.Attacks.Count == 0)
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

    // Attacking while still moving looks/reads wrong and races the Animator's locomotion transition
    // against the attack trigger - only ever attack once the enemy has actually come to a stop.
    if (!enemy.IsMoving)
    {
      IAttack matchingAttack = enemy.Attacks
        .Where(a => distance >= a.MinRange && distance <= a.MaxRange
          && angle >= a.MinAngle && angle <= a.MaxAngle)
        .OrderByDescending(a => a.Priority)
        .FirstOrDefault();

      if (matchingAttack != null)
      {
        enemy.ChangeState(new AttackState(matchingAttack));
        return;
      }
    }

    // Nothing in range right now - still move toward the preferred attack's range band so the enemy is
    // in position the moment it qualifies, instead of standing still.
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
