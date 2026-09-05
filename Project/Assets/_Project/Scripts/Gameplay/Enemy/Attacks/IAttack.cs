using System;

public interface IAttack
{
  float MinRange { get; }
  float MaxRange { get; }
  float MinAngle { get; }
  float MaxAngle { get; }
  int Priority { get; }

  float SelectionChance { get; }

  bool CanExecute { get; }

  void Execute(EnemyController enemy);

  void Interrupt();

  event Action OnAttackComplete;
}
