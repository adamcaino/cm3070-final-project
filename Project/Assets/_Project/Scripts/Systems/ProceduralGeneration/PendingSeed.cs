// Carries a seed value across a scene reload (e.g. Game Over "New Game" / "Load Seed") since a
// plain static field survives LoadScene without needing a DontDestroyOnLoad singleton.
public static class PendingSeed
{
  public static bool HasValue { get; private set; }
  public static int Value { get; private set; }

  // Stores a seed for the next dungeon scene initialization.
  public static void Set(int seed)
  {
    Value = seed;
    HasValue = true;
  }

  // Returns and clears the pending seed so it is used only once.
  public static bool Consume(out int seed)
  {
    seed = Value;
    bool hadValue = HasValue;
    HasValue = false;
    return hadValue;
  }
}
