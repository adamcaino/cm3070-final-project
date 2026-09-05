using UnityEngine;

// Terminal, non-death state entered when the player dies: stops the enemy where it stands and plays
// its looping Victory animation. Unlike DeadState, colliders and the agent stay enabled since the
// enemy isn't dead - it's just done fighting.
public class VictoryState : IEnemyState
{
  static readonly int VictoryParam = Animator.StringToHash("victory");

  public void Enter(EnemyController enemy)
  {
    if (enemy.Agent.isActiveAndEnabled && enemy.Agent.isOnNavMesh)
    {
      enemy.Agent.isStopped = true;
    }

    foreach (IAttack attack in enemy.Attacks)
    {
      attack.Interrupt();
    }

    enemy.Animator?.SetTrigger(VictoryParam);
  }

  public void Tick(EnemyController enemy) { }

  public void Exit(EnemyController enemy) { }
}
