using System;

// Cross-scene signal: DungeonLevelGenerator (gameplay scene) raises this once the full generation
// pipeline finishes; DungeonEntryFadeIn (UI scene, loaded additively) listens for it and fades the
// screen in, same pattern as GameOverSignal.
public static class DungeonReadySignal
{
  public static event Action Raised;
  public static bool IsReady { get; private set; }

  public static void Reset()
  {
    IsReady = false;
  }

  public static void Raise()
  {
    IsReady = true;
    Raised?.Invoke();
  }
}
