// Carries a seed value across a scene reload (e.g. Game Over "New Game" / "Load Seed") since a
// plain static field survives LoadScene without needing a DontDestroyOnLoad singleton.
public static class PendingSeed
{
  public static bool HasValue { get; private set; }
  public static int Value { get; private set; }

  public static void Set(int seed)
  {
    Value = seed;
    HasValue = true;
  }

  public static bool Consume(out int seed)
  {
    seed = Value;
    bool hadValue = HasValue;
    HasValue = false;
    return hadValue;
  }
}
