using UnityEngine;








// Stops the enemy movement while an attack runs and returns it to positioning afterward.
public class AttackState : IEnemyState
{
  readonly IAttack attack;
  bool attackComplete;

  // Stores the attack selected by the enemy state machine.
  public AttackState(IAttack attack)
  {
    this.attack = attack;
  }

  // Stops navigation, subscribes to attack completion, and starts the selected attack.
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

  // Faces the player and changes to positioning once the attack completes.
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

  // Removes the completion subscription and restores normal agent rotation and movement.
  public void Exit(EnemyController enemy)
  {
    attack.OnAttackComplete -= HandleAttackComplete;

    if (enemy.Agent.isActiveAndEnabled && enemy.Agent.isOnNavMesh)
    {
      enemy.Agent.isStopped = false;
    }

    enemy.Agent.updateRotation = true;
  }

  // Records that the selected attack has finished executing.
  void HandleAttackComplete() => attackComplete = true;
}
