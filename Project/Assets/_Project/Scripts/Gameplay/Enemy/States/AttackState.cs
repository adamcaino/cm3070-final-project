using UnityEngine;

/// <summary>
/// Fires the chosen attack once on entry, holds still and faces the player until the attack's own
/// OnAttackComplete fires, then hands back to PositionState to re-evaluate. No timer of any kind -
/// completion timing lives entirely in the attack's animation events. The actual hit-dealing (melee
/// hitbox window, projectile spawn) lives on the attack itself via Animation Events/Execute - this state
/// only owns movement/facing during the attack.
/// </summary>
public class AttackState : IEnemyState
{
  readonly IAttack attack;
  bool attackComplete;

  public AttackState(IAttack attack)
  {
    this.attack = attack;
  }

  public void Enter(EnemyController enemy)
  {
    enemy.Agent.isStopped = true;
    enemy.Agent.updateRotation = false;
    enemy.Agent.ResetPath();
    attackComplete = false;
    attack.OnAttackComplete += HandleAttackComplete;
    attack.Execute(enemy);
  }

  public void Tick(EnemyController enemy)
  {
    if (enemy.Player != null)
    {
      enemy.FaceTowards(enemy.Player.position);
    }

    if (attackComplete)
    {
      enemy.ChangeState(new PositionState());
    }
  }

  public void Exit(EnemyController enemy)
  {
    attack.OnAttackComplete -= HandleAttackComplete;
    enemy.Agent.isStopped = false;
    enemy.Agent.updateRotation = true;
  }

  void HandleAttackComplete() => attackComplete = true;
}
