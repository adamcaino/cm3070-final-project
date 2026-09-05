using UnityEngine;








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
    if (enemy.Agent.isActiveAndEnabled && enemy.Agent.isOnNavMesh)
    {
      enemy.Agent.isStopped = true;
      enemy.Agent.ResetPath();
    }
    enemy.Agent.updateRotation = false;
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

    if (enemy.Agent.isActiveAndEnabled && enemy.Agent.isOnNavMesh)
    {
      enemy.Agent.isStopped = false;
    }

    enemy.Agent.updateRotation = true;
  }

  void HandleAttackComplete() => attackComplete = true;
}
