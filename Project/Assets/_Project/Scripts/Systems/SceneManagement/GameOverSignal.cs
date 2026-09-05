using System;

// Cross-scene signal: PlayerDeathHandler (gameplay scene) raises this once the death delay elapses;
// GameOverScreen (UI scene, loaded additively) listens for it and shows itself.
public static class GameOverSignal
{
  public static event Action<string> Raised;
  public static event Action<string> VictoryRaised;
  public static event Action<string> VictoryScreenShown;

  public static void Raise(string gameplaySceneName) => Raised?.Invoke(gameplaySceneName);
  public static void RaiseVictory(string gameplaySceneName) => VictoryRaised?.Invoke(gameplaySceneName);
  public static void RaiseVictoryScreenShown(string gameplaySceneName) => VictoryScreenShown?.Invoke(gameplaySceneName);
}
