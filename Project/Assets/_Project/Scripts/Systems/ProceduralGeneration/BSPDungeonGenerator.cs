using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 2D proof-of-concept BSP dungeon generator for comparing room-based procedural generation.
/// Grey tiles are floor, black tiles are walls, and red tiles indicate door positions.
/// </summary>
public class BSPDungeonGenerator : DungeonGridGenerator2D
{
  [Header("BSP Settings")]
  [SerializeField, Min(1)] int maxDepth = 4;
  [SerializeField, Min(8)] int minLeafSize = 12;
  [SerializeField, Min(3)] int minimumRoomSize = 5;
  [SerializeField, Min(3)] int maximumRoomSize = 20;
  [SerializeField, Min(0)] int minCorridorLength = 2;

  [Tooltip("How many floor tiles wide a corridor's open passage is. The doorway into a room stays a single tile regardless - SealRoomPerimeters trims the wider carve back down to just the one marked Door cell at the threshold, so widening this only affects the corridor's body, not room entrances.")]
  [SerializeField, Min(1)] int corridorWidth = 3;

  [Header("Points of Interest")]
  [Tooltip("How many rooms (besides spawn and boss) get tagged as loot rooms.")]
  [SerializeField, Min(0)] int lootRoomCount = 3;

  public IReadOnlyList<DungeonRoomInfo> LastRooms { get; private set; }

  readonly List<RectInt> roomBounds = new List<RectInt>();
  readonly List<List<int>> roomAdjacency = new List<List<int>>();
  readonly Dictionary<Vector2Int, List<int>> roomPerimeterOwners = new Dictionary<Vector2Int, List<int>>();

  // Two sibling rooms can each be placed as close as RoomPadding cells from their shared leaf
  // boundary, so the worst-case gap between them is 2 * RoomPadding. Doors occupy the first and
  // last cell of that gap, leaving (2 * RoomPadding) - 2 cells of pure corridor floor - deriving
  // RoomPadding from minCorridorLength guarantees that stays >= minCorridorLength.
  int RoomPadding => Mathf.Max(1, Mathf.CeilToInt((minCorridorLength + 2) / 2f));

  protected override void OnValidate()
  {
    base.OnValidate();
    maxDepth = Mathf.Max(1, maxDepth);
    minLeafSize = Mathf.Max(8, minLeafSize);
    minimumRoomSize = Mathf.Max(3, minimumRoomSize);
    maximumRoomSize = Mathf.Max(minimumRoomSize, maximumRoomSize);
    minCorridorLength = Mathf.Max(0, minCorridorLength);
    corridorWidth = Mathf.Max(1, corridorWidth);
  }

  protected override TileType[,] BuildMap(int seed)
  {
    System.Random random = new System.Random(seed);
    TileType[,] map = CreateFilledMap(TileType.Wall);

    roomBounds.Clear();
    roomAdjacency.Clear();
    roomPerimeterOwners.Clear();

    RectInt rootArea = new RectInt(1, 1, GridWidth - 2, GridHeight - 2);
    BspNode root = SplitRecursively(rootArea, 0, random);

    CreateRooms(root, random, map);
    BuildRoomPerimeterOwners();
    ConnectSiblingRooms(root, random, map);
    SealRoomPerimeters(map);
    AssignRoomRoles(random);

    return map;
  }

  BspNode SplitRecursively(RectInt area, int depth, System.Random random)
  {
    BspNode node = new BspNode(area);

    if (depth >= maxDepth)
    {
      return node;
    }

    bool canSplitHorizontally = area.height >= minLeafSize * 2;
    bool canSplitVertically = area.width >= minLeafSize * 2;

    if (!canSplitHorizontally && !canSplitVertically)
    {
      return node;
    }

    bool splitHorizontally = ChooseSplitOrientation(area, canSplitHorizontally, canSplitVertically, random);

    if (splitHorizontally)
    {
      int splitY = random.Next(minLeafSize, area.height - minLeafSize + 1);
      RectInt bottom = new RectInt(area.xMin, area.yMin, area.width, splitY);
      RectInt top = new RectInt(area.xMin, area.yMin + splitY, area.width, area.height - splitY);

      node.Left = SplitRecursively(bottom, depth + 1, random);
      node.Right = SplitRecursively(top, depth + 1, random);
    }
    else
    {
      int splitX = random.Next(minLeafSize, area.width - minLeafSize + 1);
      RectInt left = new RectInt(area.xMin, area.yMin, splitX, area.height);
      RectInt right = new RectInt(area.xMin + splitX, area.yMin, area.width - splitX, area.height);

      node.Left = SplitRecursively(left, depth + 1, random);
      node.Right = SplitRecursively(right, depth + 1, random);
    }

    return node;
  }

  bool ChooseSplitOrientation(RectInt area, bool canSplitHorizontally, bool canSplitVertically, System.Random random)
  {
    if (!canSplitHorizontally)
    {
      return false;
    }

    if (!canSplitVertically)
    {
      return true;
    }

    float ratio = (float)area.width / area.height;
    if (ratio >= 1.25f)
    {
      return false;
    }

    if (ratio <= 0.8f)
    {
      return true;
    }

    return random.NextDouble() > 0.5;
  }

  void CreateRooms(BspNode node, System.Random random, TileType[,] map)
  {
    if (node == null)
    {
      return;
    }

    if (!node.IsLeaf)
    {
      CreateRooms(node.Left, random, map);
      CreateRooms(node.Right, random, map);
      return;
    }

    int maxRoomWidth = Mathf.Min(maximumRoomSize, Mathf.Max(minimumRoomSize, node.Area.width - (RoomPadding * 2)));
    int maxRoomHeight = Mathf.Min(maximumRoomSize, Mathf.Max(minimumRoomSize, node.Area.height - (RoomPadding * 2)));

    int roomWidth = random.Next(minimumRoomSize, maxRoomWidth + 1);
    int roomHeight = random.Next(minimumRoomSize, maxRoomHeight + 1);

    int roomXMin = node.Area.xMin + RoomPadding;
    int roomYMin = node.Area.yMin + RoomPadding;
    int roomXMax = Mathf.Max(roomXMin, node.Area.xMax - RoomPadding - roomWidth);
    int roomYMax = Mathf.Max(roomYMin, node.Area.yMax - RoomPadding - roomHeight);

    int roomX = random.Next(roomXMin, roomXMax + 1);
    int roomY = random.Next(roomYMin, roomYMax + 1);

    node.Room = new RectInt(roomX, roomY, roomWidth, roomHeight);
    node.HasRoom = true;
    node.RoomIndex = roomBounds.Count;
    roomBounds.Add(node.Room);
    roomAdjacency.Add(new List<int>());

    for (int x = node.Room.xMin; x < node.Room.xMax; x++)
    {
      for (int y = node.Room.yMin; y < node.Room.yMax; y++)
      {
        map[x, y] = TileType.Floor;
      }
    }
  }

  // Every room's own wall ring is recorded up front, before any corridor gets carved, so corridor
  // carving can tell "this room's own wall, safe to breach for its own doorway" apart from "some other
  // room's wall that this corridor is merely passing near" and protect the latter.
  void BuildRoomPerimeterOwners()
  {
    for (int i = 0; i < roomBounds.Count; i++)
    {
      foreach (Vector2Int cell in GetRoomPerimeterCells(roomBounds[i]))
      {
        if (!roomPerimeterOwners.TryGetValue(cell, out List<int> owners))
        {
          owners = new List<int>();
          roomPerimeterOwners[cell] = owners;
        }

        owners.Add(i);
      }
    }
  }

  static List<Vector2Int> GetRoomPerimeterCells(RectInt bounds)
  {
    List<Vector2Int> cells = new List<Vector2Int>();

    for (int x = bounds.xMin; x < bounds.xMax; x++)
    {
      cells.Add(new Vector2Int(x, bounds.yMin - 1));
      cells.Add(new Vector2Int(x, bounds.yMax));
    }

    for (int y = bounds.yMin; y < bounds.yMax; y++)
    {
      cells.Add(new Vector2Int(bounds.xMin - 1, y));
      cells.Add(new Vector2Int(bounds.xMax, y));
    }

    // The 4 diagonal corners just outside the rectangle aren't covered by either loop above (both
    // stop at the room's own x/y range) - without these, a corridor leg running along another room's
    // wall row can still punch through right at the far end of that row, at the corner.
    cells.Add(new Vector2Int(bounds.xMin - 1, bounds.yMin - 1));
    cells.Add(new Vector2Int(bounds.xMax, bounds.yMin - 1));
    cells.Add(new Vector2Int(bounds.xMin - 1, bounds.yMax));
    cells.Add(new Vector2Int(bounds.xMax, bounds.yMax));

    return cells;
  }

  // Final safety net, run once after every corridor for the whole dungeon has been carved and every
  // legitimate crossing has been marked as a Door: any ring cell that ended up Floor instead of Wall or
  // Door is - by construction - an accidental graze rather than an intended connection, since every
  // intended connection already got its own explicit Door marker elsewhere. Sealing those cells back to
  // Wall can never disconnect a real corridor; it only ever removes cells nothing depended on.
  void SealRoomPerimeters(TileType[,] map)
  {
    foreach (RectInt bounds in roomBounds)
    {
      foreach (Vector2Int cell in GetRoomPerimeterCells(bounds))
      {
        if (GetTile(map, cell.x, cell.y) == TileType.Floor)
        {
          SetTile(map, cell.x, cell.y, TileType.Wall);
        }
      }
    }
  }

  void ConnectSiblingRooms(BspNode node, System.Random random, TileType[,] map)
  {
    if (node == null || node.IsLeaf)
    {
      return;
    }

    ConnectSiblingRooms(node.Left, random, map);
    ConnectSiblingRooms(node.Right, random, map);

    if (!TryFindClosestRoomPair(node.Left, node.Right, out RectInt leftRoom, out int leftIndex, out RectInt rightRoom, out int rightIndex))
    {
      return;
    }

    roomAdjacency[leftIndex].Add(rightIndex);
    roomAdjacency[rightIndex].Add(leftIndex);

    Vector2Int start = GetRoomCenter(leftRoom);
    Vector2Int end = GetRoomCenter(rightRoom);

    bool useHorizontalFirst = random.NextDouble() > 0.5;
    List<Vector2Int> preferredPath = useHorizontalFirst
      ? BuildHorizontalThenVerticalPath(start, end, leftRoom, rightRoom)
      : BuildVerticalThenHorizontalPath(start, end, leftRoom, rightRoom);

    // If an unrelated third room happens to sit directly on this route, the preferred path would get
    // blocked mid-corridor by that room's own wall protection, leaving a dead end. The other L-shape
    // routes through completely different cells, so it's usually clear even when this one isn't.
    List<Vector2Int> corridorPath = preferredPath;
    if (IsPathBlocked(preferredPath, leftIndex, rightIndex))
    {
      List<Vector2Int> alternatePath = useHorizontalFirst
        ? BuildVerticalThenHorizontalPath(start, end, leftRoom, rightRoom)
        : BuildHorizontalThenVerticalPath(start, end, leftRoom, rightRoom);

      if (!IsPathBlocked(alternatePath, leftIndex, rightIndex))
      {
        corridorPath = alternatePath;
      }
    }

    CarveCorridorPath(corridorPath, map, leftIndex, rightIndex);
    TryPlaceDoorAtCorridorBoundary(leftRoom, corridorPath, true, map);
    TryPlaceDoorAtCorridorBoundary(rightRoom, corridorPath, false, map);
  }

  bool IsPathBlocked(List<Vector2Int> path, int allowedRoomA, int allowedRoomB)
  {
    foreach (Vector2Int cell in path)
    {
      if (IsOtherRoomsWall(cell, allowedRoomA, allowedRoomB))
      {
        return true;
      }
    }

    return false;
  }

  bool TryFindClosestRoomPair(BspNode leftNode, BspNode rightNode, out RectInt leftRoom, out int leftIndex, out RectInt rightRoom, out int rightIndex)
  {
    List<BspNode> leftRooms = new List<BspNode>();
    List<BspNode> rightRooms = new List<BspNode>();
    CollectRooms(leftNode, leftRooms);
    CollectRooms(rightNode, rightRooms);

    leftRoom = default;
    leftIndex = -1;
    rightRoom = default;
    rightIndex = -1;

    if (leftRooms.Count == 0 || rightRooms.Count == 0)
    {
      return false;
    }

    int bestDistance = int.MaxValue;

    foreach (BspNode first in leftRooms)
    {
      Vector2Int firstCenter = GetRoomCenter(first.Room);

      foreach (BspNode second in rightRooms)
      {
        Vector2Int secondCenter = GetRoomCenter(second.Room);
        int distance = Mathf.Abs(firstCenter.x - secondCenter.x) + Mathf.Abs(firstCenter.y - secondCenter.y);

        if (distance >= bestDistance)
        {
          continue;
        }

        bestDistance = distance;
        leftRoom = first.Room;
        leftIndex = first.RoomIndex;
        rightRoom = second.Room;
        rightIndex = second.RoomIndex;
      }
    }

    return true;
  }

  void CollectRooms(BspNode node, List<BspNode> rooms)
  {
    if (node == null)
    {
      return;
    }

    if (node.HasRoom)
    {
      rooms.Add(node);
    }

    CollectRooms(node.Left, rooms);
    CollectRooms(node.Right, rooms);
  }

  // A plain 2-segment L (elbow at end.x/end.y) puts the turn wherever the target room's center
  // happens to be - if that's only a cell or two past the wall the path just exited through, the
  // second leg then runs straight along that same wall for a stretch instead of turning cleanly away
  // from it, which is what was carving extra holes in the source room's own wall (only the single
  // actual crossing point ever gets marked as a door; every other cell the leg grazes along the wall
  // just silently becomes floor). Clamping the elbow to sit at least CorridorTurnClearance cells past
  // the exit wall - and adding a third segment to re-align with the target's center - guarantees a
  // straight run of open corridor before any turn. When the natural elbow already clears the wall (the
  // common case), the clamp and the extra segment are no-ops and the path stays a simple 2-segment L.
  const int CorridorTurnClearance = 1;

  List<Vector2Int> BuildHorizontalThenVerticalPath(Vector2Int start, Vector2Int end, RectInt sourceRoom, RectInt destRoom)
  {
    int elbowX = ChooseClearedElbow(start.x, end.x, sourceRoom.xMin, sourceRoom.xMax, destRoom.xMin, destRoom.xMax, GridWidth);

    List<Vector2Int> path = new List<Vector2Int>();
    AddLineToPath(path, start, new Vector2Int(elbowX, start.y));
    AddLineToPath(path, new Vector2Int(elbowX, start.y), new Vector2Int(elbowX, end.y));
    AddLineToPath(path, new Vector2Int(elbowX, end.y), end);
    return path;
  }

  List<Vector2Int> BuildVerticalThenHorizontalPath(Vector2Int start, Vector2Int end, RectInt sourceRoom, RectInt destRoom)
  {
    int elbowY = ChooseClearedElbow(start.y, end.y, sourceRoom.yMin, sourceRoom.yMax, destRoom.yMin, destRoom.yMax, GridHeight);

    List<Vector2Int> path = new List<Vector2Int>();
    AddLineToPath(path, start, new Vector2Int(start.x, elbowY));
    AddLineToPath(path, new Vector2Int(start.x, elbowY), new Vector2Int(end.x, elbowY));
    AddLineToPath(path, new Vector2Int(end.x, elbowY), end);
    return path;
  }

  // Clears the source room's wall (as above), and also tries to avoid landing inside the destination
  // room's own span on that axis - landing there makes the middle leg enter the destination early,
  // through a side wall near a corner, instead of through its center on the final leg. But with rooms
  // packed close together (small leaves, tight padding), there may not be a column/row that clears both
  // rooms at once - source clearance is the one that must never be given up (losing it reintroduces the
  // wall-hugging bug), so the destination pull-back is only applied when it doesn't creep back into the
  // source's own cleared zone; otherwise this settles for an off-center (but still single, clean) entry.
  // Finally clamped to the grid itself - a room near the map edge can have nowhere to push the clearance
  // into, and an elbow that lands outside the grid isn't just unclamped, it's silently uncarvable: every
  // cell beyond it gets dropped by the bounds check during carving, stranding the corridor at the door.
  int ChooseClearedElbow(int startCoord, int endCoord, int sourceMin, int sourceMax, int destMin, int destMax, int gridLength)
  {
    if (endCoord > startCoord)
    {
      int minClearOfSource = sourceMax + CorridorTurnClearance;
      int elbow = Mathf.Max(endCoord, minClearOfSource);

      if (elbow >= destMin && elbow < destMax)
      {
        int pulledBack = destMin - 1 - CorridorTurnClearance;
        if (pulledBack >= minClearOfSource)
        {
          elbow = pulledBack;
        }
      }

      return Mathf.Min(elbow, gridLength - 1);
    }

    if (endCoord < startCoord)
    {
      int maxClearOfSource = sourceMin - 1 - CorridorTurnClearance;
      int elbow = Mathf.Min(endCoord, maxClearOfSource);

      if (elbow >= destMin && elbow < destMax)
      {
        int pulledBack = destMax + CorridorTurnClearance;
        if (pulledBack <= maxClearOfSource)
        {
          elbow = pulledBack;
        }
      }

      return Mathf.Max(elbow, 0);
    }

    return endCoord;
  }

  void AddLineToPath(List<Vector2Int> path, Vector2Int from, Vector2Int to)
  {
    Vector2Int current = from;
    Vector2Int step = new Vector2Int(Math.Sign(to.x - from.x), Math.Sign(to.y - from.y));

    if (path.Count == 0 || path[path.Count - 1] != current)
    {
      path.Add(current);
    }

    while (current != to)
    {
      current += step;
      if (path[path.Count - 1] != current)
      {
        path.Add(current);
      }
    }
  }

  void CarveCorridorPath(List<Vector2Int> corridorPath, TileType[,] map, int allowedRoomA, int allowedRoomB)
  {
    foreach (Vector2Int cell in corridorPath)
    {
      CarveBrush(cell, map, allowedRoomA, allowedRoomB);
    }
  }

  void CarveBrush(Vector2Int center, TileType[,] map, int allowedRoomA, int allowedRoomB)
  {
    int negativeOffset = corridorWidth / 2;
    int positiveOffset = corridorWidth - negativeOffset;

    for (int x = center.x - negativeOffset; x < center.x + positiveOffset; x++)
    {
      for (int y = center.y - negativeOffset; y < center.y + positiveOffset; y++)
      {
        // A room often sits on more than one connection (it can be the closest room for multiple
        // sibling pairings up the BSP tree), so a later corridor sharing that room is allowed to carve
        // across its ring too - without this check it could sweep back over an earlier connection's
        // already-placed Door and silently downgrade it to Floor, orphaning that door (which the final
        // seal pass would then wall back up, severing a connection that was actually real).
        if (!IsInsideMap(x, y) || GetTile(map, x, y) == TileType.Door || IsOtherRoomsWall(new Vector2Int(x, y), allowedRoomA, allowedRoomB))
        {
          continue;
        }

        map[x, y] = TileType.Floor;
      }
    }
  }

  // A corridor is only allowed to breach the wall ring of the two rooms it's actually connecting -
  // its own doorway. Any other room's wall ring cell the path happens to graze along the way is left
  // untouched, so a corridor can never open a gap into a room it isn't meant to connect to. A cell can
  // belong to more than one room's ring when rooms sit close together, so this blocks as soon as ANY
  // owner of the cell falls outside the allowed pair - not just when the sole owner does.
  bool IsOtherRoomsWall(Vector2Int cell, int allowedRoomA, int allowedRoomB)
  {
    if (!roomPerimeterOwners.TryGetValue(cell, out List<int> owners))
    {
      return false;
    }

    foreach (int owner in owners)
    {
      if (owner != allowedRoomA && owner != allowedRoomB)
      {
        return true;
      }
    }

    return false;
  }

  void TryPlaceDoorAtCorridorBoundary(RectInt room, List<Vector2Int> corridorPath, bool fromStart, TileType[,] map)
  {
    if (corridorPath == null || corridorPath.Count < 2)
    {
      return;
    }

    if (fromStart)
    {
      for (int i = 1; i < corridorPath.Count; i++)
      {
        Vector2Int previous = corridorPath[i - 1];
        Vector2Int current = corridorPath[i];

        if (IsInsideRoom(room, previous) && !IsInsideRoom(room, current))
        {
          PlaceDoorIfConnected(map, previous, current);
          return;
        }
      }

      return;
    }

    for (int i = corridorPath.Count - 2; i >= 0; i--)
    {
      Vector2Int current = corridorPath[i];
      Vector2Int next = corridorPath[i + 1];

      if (!IsInsideRoom(room, current) && IsInsideRoom(room, next))
      {
        PlaceDoorIfConnected(map, next, current);
        return;
      }
    }
  }

  // Always marks the crossing - even if another door already sits right next to it. An earlier version
  // suppressed the marking when a neighboring door existed to avoid two doors touching, but the corridor
  // had already been carved to Floor by that point, so suppressing the *marking* left a silent gap
  // (Floor, no door, no wall) instead of actually preventing anything. Two doors sitting side by side is
  // a much smaller problem than an invisible hole in a wall, so this always marks the real crossing.
  void PlaceDoorIfConnected(TileType[,] map, Vector2Int roomCell, Vector2Int corridorCell)
  {
    if (!IsInsideMap(roomCell.x, roomCell.y) || !IsInsideMap(corridorCell.x, corridorCell.y))
    {
      return;
    }

    TileType roomTile = map[roomCell.x, roomCell.y];
    if (roomTile == TileType.Floor || roomTile == TileType.Door)
    {
      map[corridorCell.x, corridorCell.y] = TileType.Door;
    }
  }

  bool IsInsideRoom(RectInt room, Vector2Int cell)
  {
    return cell.x >= room.xMin && cell.x < room.xMax && cell.y >= room.yMin && cell.y < room.yMax;
  }

  Vector2Int GetRoomCenter(RectInt room)
  {
    return new Vector2Int(room.xMin + (room.width / 2), room.yMin + (room.height / 2));
  }

  // Spawn is a random room; boss is whichever room sits furthest from spawn along the room graph (not
  // straight-line distance), so it's guaranteed to be the most "out of the way" room reachable from the
  // start - which reads as "end of the level" regardless of the BSP tree's actual shape. Loot rooms are
  // picked from whatever's left over, so they never double up as the spawn or boss room.
  void AssignRoomRoles(System.Random random)
  {
    LastRooms = null;

    if (roomBounds.Count == 0)
    {
      return;
    }

    int spawnIndex = random.Next(roomBounds.Count);
    int[] distancesFromSpawn = ComputeRoomDistances(spawnIndex);
    int bossIndex = FindFarthestRoom(distancesFromSpawn, spawnIndex);
    int[] distancesFromBoss = ComputeRoomDistances(bossIndex);
    int spawnToBossDistance = Mathf.Max(0, distancesFromSpawn[bossIndex]);
    List<int> lootIndices = ChooseLootRooms(distancesFromSpawn, spawnIndex, bossIndex, random);

    List<DungeonRoomInfo> rooms = new List<DungeonRoomInfo>(roomBounds.Count);

    for (int i = 0; i < roomBounds.Count; i++)
    {
      RoomRole role = RoomRole.Normal;

      if (i == spawnIndex)
      {
        role = RoomRole.Spawn;
      }
      else if (i == bossIndex)
      {
        role = RoomRole.Boss;
      }
      else if (lootIndices.Contains(i))
      {
        role = RoomRole.Loot;
      }

      rooms.Add(new DungeonRoomInfo
      {
        RoomId = i,
        Bounds = roomBounds[i],
        Center = GetRoomCenter(roomBounds[i]),
        Role = role,
        DistanceFromSpawn = distancesFromSpawn[i],
        DistanceFromBoss = distancesFromBoss[i],
        SpawnToBossDistance = spawnToBossDistance
      });
    }

    LastRooms = rooms;
  }

  int[] ComputeRoomDistances(int startIndex)
  {
    int[] distances = new int[roomBounds.Count];
    for (int i = 0; i < distances.Length; i++)
    {
      distances[i] = -1;
    }

    Queue<int> frontier = new Queue<int>();
    distances[startIndex] = 0;
    frontier.Enqueue(startIndex);

    while (frontier.Count > 0)
    {
      int current = frontier.Dequeue();

      foreach (int neighbor in roomAdjacency[current])
      {
        if (distances[neighbor] != -1)
        {
          continue;
        }

        distances[neighbor] = distances[current] + 1;
        frontier.Enqueue(neighbor);
      }
    }

    return distances;
  }

  int FindFarthestRoom(int[] distances, int excludeIndex)
  {
    int bestIndex = excludeIndex;
    int bestDistance = -1;

    for (int i = 0; i < distances.Length; i++)
    {
      if (i == excludeIndex || distances[i] <= bestDistance)
      {
        continue;
      }

      bestDistance = distances[i];
      bestIndex = i;
    }

    return bestIndex;
  }

  List<int> ChooseLootRooms(int[] distances, int spawnIndex, int bossIndex, System.Random random)
  {
    List<int> candidates = new List<int>();

    for (int i = 0; i < roomBounds.Count; i++)
    {
      if (i == spawnIndex || i == bossIndex || distances[i] < 0)
      {
        continue;
      }

      candidates.Add(i);
    }

    List<int> chosen = new List<int>();
    int count = Mathf.Min(lootRoomCount, candidates.Count);

    for (int i = 0; i < count; i++)
    {
      int pick = random.Next(candidates.Count);
      chosen.Add(candidates[pick]);
      candidates.RemoveAt(pick);
    }

    return chosen;
  }

  sealed class BspNode
  {
    public readonly RectInt Area;
    public BspNode Left;
    public BspNode Right;
    public RectInt Room;
    public bool HasRoom;
    public int RoomIndex = -1;

    public bool IsLeaf => Left == null && Right == null;

    public BspNode(RectInt area)
    {
      Area = area;
    }
  }
}
