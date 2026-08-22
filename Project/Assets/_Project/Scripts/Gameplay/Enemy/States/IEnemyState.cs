/// <summary>
/// One state per enemy behaviour (Roam, Position, Attack, Dead). Each state owns its own transition
/// logic - e.g. RoamState decides when to move to PositionState - rather than a central switch statement
/// in EnemyController deciding for everyone.
/// </summary>
public interface IEnemyState
{
  void Enter(EnemyController enemy);
  void Tick(EnemyController enemy);
  void Exit(EnemyController enemy);
}
