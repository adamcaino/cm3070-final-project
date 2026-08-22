using System;


public interface IAttack
{
  float MinRange { get; }
  float MaxRange { get; }
  float MinAngle { get; }
  float MaxAngle { get; }
  int Priority { get; }

  void Execute(EnemyController enemy);

  event Action OnAttackComplete;
}
