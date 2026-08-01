using UnityEngine;

/// <summary>
/// Shared helpers for turning a Direction flag into world-space facing info, used by both the tile
/// placer and the prop placer so their rotation conventions stay identical.
/// </summary>
public static class DirectionUtility
{
  public static Vector3 GetFacingDirection(Direction direction)
  {
    if ((direction & Direction.North) != Direction.None)
    {
      return Vector3.forward;
    }

    if ((direction & Direction.East) != Direction.None)
    {
      return Vector3.right;
    }

    if ((direction & Direction.South) != Direction.None)
    {
      return Vector3.back;
    }

    if ((direction & Direction.West) != Direction.None)
    {
      return Vector3.left;
    }

    return Vector3.zero;
  }

  public static Quaternion GetFacingRotation(Direction direction)
  {
    Vector3 facing = GetFacingDirection(direction);
    return facing == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(facing, Vector3.up);
  }

  public static Direction GetFirstCardinal(Direction direction)
  {
    if ((direction & Direction.North) != Direction.None)
    {
      return Direction.North;
    }

    if ((direction & Direction.East) != Direction.None)
    {
      return Direction.East;
    }

    if ((direction & Direction.South) != Direction.None)
    {
      return Direction.South;
    }

    if ((direction & Direction.West) != Direction.None)
    {
      return Direction.West;
    }

    return Direction.None;
  }

  public static Direction GetOpposite(Direction cardinalDirection)
  {
    switch (cardinalDirection)
    {
      case Direction.North:
        return Direction.South;
      case Direction.South:
        return Direction.North;
      case Direction.East:
        return Direction.West;
      case Direction.West:
        return Direction.East;
      default:
        return Direction.None;
    }
  }

  public static (Direction PerpendicularA, Direction PerpendicularB) GetPerpendicularCardinals(Direction cardinalDirection)
  {
    return cardinalDirection == Direction.North || cardinalDirection == Direction.South
      ? (Direction.East, Direction.West)
      : (Direction.North, Direction.South);
  }
}
