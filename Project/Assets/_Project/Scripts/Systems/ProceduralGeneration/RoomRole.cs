// Identifies how a generated room is used by later placement passes.
public enum RoomRole
{
  // Receives ordinary enemy placement.
  Normal,
  // Receives the player spawn portal.
  Spawn,
  // Receives the boss and encounter trigger.
  Boss,
  // Receives a loot point of interest.
  Loot
}
