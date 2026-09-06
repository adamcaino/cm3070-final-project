using System;

// Defines the range, selection, execution, and completion contract for enemy attacks.
public interface IAttack
{
  // Minimum distance at which the attack can be selected.
  float MinRange { get; }
  // Maximum distance at which the attack can be selected.
  float MaxRange { get; }
  // Minimum facing angle required to select the attack.
  float MinAngle { get; }
  // Maximum facing angle required to select the attack.
  float MaxAngle { get; }
  // Priority used when multiple attacks are available.
  int Priority { get; }

  // Probability used when the attack is the highest-priority valid choice.
  float SelectionChance { get; }

  // Reports whether the attack is currently available.
  bool CanExecute { get; }

  // Starts the attack for the supplied enemy.
  void Execute(EnemyController enemy);

  // Stops or cancels the active attack.
  void Interrupt();

  // Raised when the attack has completed its current execution.
  event Action OnAttackComplete;
}
