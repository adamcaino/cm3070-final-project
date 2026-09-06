using UnityEngine;

// Terminal, non-death state entered when the player dies: stops the enemy where it stands and plays
// its looping Victory animation. Unlike DeadState, colliders and the agent stay enabled since the
// enemy isn't dead - it's just done fighting.
// Stops combat and plays the victory animation after the player dies.
public class VictoryState : IEnemyState
{
  static readonly int VictoryParam = Animator.StringToHash("victory");

  // Stops movement, interrupts active attacks, and triggers the victory animation.
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

  // Keeps the enemy in its terminal victory state.
  public void Tick(EnemyController enemy) { }

  // Provides the state-machine exit hook; the victory state has no temporary setup to remove.
  public void Exit(EnemyController enemy) { }
}
