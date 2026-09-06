




// Defines the lifecycle methods used by the enemy state machine.
public interface IEnemyState
{
  // Enters the state and applies its initial enemy configuration.
  void Enter(EnemyController enemy);

  // Updates the state for the current frame.
  void Tick(EnemyController enemy);

  // Exits the state and removes its temporary configuration or subscriptions.
  void Exit(EnemyController enemy);
}
