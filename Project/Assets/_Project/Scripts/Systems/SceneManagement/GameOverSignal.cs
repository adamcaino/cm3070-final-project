using System;

// Cross-scene signal: PlayerDeathHandler (gameplay scene) raises this once the death delay elapses;
// GameOverScreen (UI scene, loaded additively) listens for it and shows itself.
public static class GameOverSignal
{
  public static event Action<string> Raised;

  public static void Raise(string gameplaySceneName) => Raised?.Invoke(gameplaySceneName);
}
