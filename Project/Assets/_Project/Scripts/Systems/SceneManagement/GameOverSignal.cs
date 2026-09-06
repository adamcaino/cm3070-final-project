using System;

// Cross-scene signal: PlayerDeathHandler (gameplay scene) raises this once the death delay elapses;
// GameOverScreen (UI scene, loaded additively) listens for it and shows itself.
public static class GameOverSignal
{
  public static event Action<string> Raised;
  public static event Action<string> VictoryRaised;
  public static event Action<string> VictoryScreenShown;

  // Notifies the UI that the player has died in the supplied gameplay scene.
  public static void Raise(string gameplaySceneName) => Raised?.Invoke(gameplaySceneName);

  // Notifies the UI that the player has won in the supplied gameplay scene.
  public static void RaiseVictory(string gameplaySceneName) => VictoryRaised?.Invoke(gameplaySceneName);

  // Notifies listeners that the victory panel has become visible.
  public static void RaiseVictoryScreenShown(string gameplaySceneName) => VictoryScreenShown?.Invoke(gameplaySceneName);
}
