using System;

// Raised the instant the player dies, ahead of the Game Over delay, so systems like enemy AI can
// react immediately instead of waiting on GameOverSignal (which is timed to the UI reveal).
public static class PlayerDiedSignal
{
  public static event Action Raised;

  // Notifies subscribers that the player has died.
  public static void Raise() => Raised?.Invoke();
}
