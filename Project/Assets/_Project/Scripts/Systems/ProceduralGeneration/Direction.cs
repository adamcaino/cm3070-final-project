using System;

[Flags]
// Represents cardinal and diagonal neighbour directions as combinable bit flags.
public enum Direction
{
  // Represents no direction.
  None = 0,
  // Points toward the positive grid Y axis.
  North = 1 << 0,
  // Points toward the positive grid X axis.
  East = 1 << 1,
  // Points toward the negative grid Y axis.
  South = 1 << 2,
  // Points toward the negative grid X axis.
  West = 1 << 3,
  // Represents the north-east diagonal neighbour.
  NorthEast = 1 << 4,
  // Represents the south-east diagonal neighbour.
  SouthEast = 1 << 5,
  // Represents the south-west diagonal neighbour.
  SouthWest = 1 << 6,
  // Represents the north-west diagonal neighbour.
  NorthWest = 1 << 7
}
