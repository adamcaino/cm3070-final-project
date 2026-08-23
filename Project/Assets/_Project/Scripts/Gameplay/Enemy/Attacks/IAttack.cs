using System;


public interface IAttack
{
  float MinRange { get; }
  float MaxRange { get; }
  float MinAngle { get; }
  float MaxAngle { get; }
  int Priority { get; }

  /// <summary>Odds (0-1) that this attack is taken when it's the highest-priority match in range - e.g. a
  /// spin attack that should only sometimes win out over a plain melee swing rather than every time.
  /// Rolled once per attack decision; a miss falls through to the next-highest-priority match. 1 (the
  /// default) always wins, matching the old priority-only behaviour.</summary>
  float SelectionChance { get; }

  /// <summary>Whether this attack is currently allowed to fire, independent of range/angle - e.g. a
  /// summon attack refusing to trigger while its cap of active summons hasn't been depleted.</summary>
  bool CanExecute { get; }

  void Execute(EnemyController enemy);

  /// <summary>Forcibly shuts down any ongoing effect of this attack (e.g. a flamethrower's VFX/hitbox)
  /// without waiting for its usual animation-driven end - for interrupts like the enemy being frozen
  /// mid-attack. No-op for attacks with nothing ongoing to stop.</summary>
  void Interrupt();

  event Action OnAttackComplete;
}
