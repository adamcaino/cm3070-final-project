using System;

// Cross-scene signal: DungeonLevelGenerator (gameplay scene) raises this once the full generation
// pipeline finishes; DungeonEntryFadeIn (UI scene, loaded additively) listens for it and fades the
// screen in, same pattern as GameOverSignal.
public static class DungeonReadySignal
{
  public static event Action Raised;
  public static bool IsReady { get; private set; }

  // Marks dungeon generation as incomplete before a new generation sequence starts.
  public static void Reset()
  {
    IsReady = false;
  }

  // Marks generation complete and notifies listeners that the dungeon can be revealed.
  public static void Raise()
  {
    IsReady = true;
    Raised?.Invoke();
  }
}
