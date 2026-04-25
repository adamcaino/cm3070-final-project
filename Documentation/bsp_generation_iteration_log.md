# BSP Generation Iteration Log

## Scope

This document records the implementation and refinement history of BSPDungeonGenerator for the 2D proof-of-concept dungeon pipeline.

---

## Iteration 1: Initial BSP implementation

### Goals

- Build a deterministic room-and-corridor dungeon generator using Binary Space Partitioning.
- Visualize output as 2D grid tiles for rapid inspection.
- Support seed-driven reproducibility.

### Work completed

- Implemented recursive BSP partition splitting with depth and minimum leaf constraints.
- Added room carving with configurable room padding and minimum room size.
- Connected sibling partitions using orthogonal L-shaped corridors.
- Added door markers in connected rooms.

### Expected output at this stage

- Structured room-based layouts.
- Grey floors, black walls, red doors.
- Reproducible generation from seed.

---

## Iteration 2: Door and corridor misalignment

### Issue discovered

Some door markers did not visually align with corridor openings. The red cell appeared near the correct room boundary but not always on the exact entry tile used by the corridor.

### Root cause

Door placement was inferred from room center direction heuristics, while corridor routing could follow either horizontal-first or vertical-first paths. These two decisions could diverge in edge cases.

### Fix applied

- Updated door placement to account for chosen corridor route.
- Added route-aware horizontal and vertical door placement logic.

### Outcome

- Door markers aligned better with actual room entries.
- Remaining edge cases were reduced but not fully eliminated.

---

## Iteration 3: Hallucinated doors

### Issue discovered

Some doors still appeared on room boundaries without a true corridor crossing (hallucinated doors).

### Root cause
Door placement still depended on inferred edge coordinates rather than verified path boundary transitions.

### Fix applied

The generator was converted to explicit path-driven placement:

- Build corridor route as a concrete ordered list of cells.
- Carve the corridor directly from this path.
- Detect room boundary transitions along the path.
- Place doors only when a room cell transitions to a non-room corridor cell (or inverse at the destination side).
- Validate adjacency by ensuring the corridor-side tile is floor or door before marking a door.

### Outcome

- Hallucinated doors removed.
- Doors now appear only where a corridor truly enters or exits a room.
- Rooms, corridors, and door markers are spatially consistent.

---

## Final BSP behavior

- Deterministic BSP room-and-corridor generation.
- Stable room layout control through partition settings.
- Corridor carving from explicit path routing.
- Door placement constrained to verified room-corridor boundaries.

---

## Validation

Build verification after changes succeeded for:

- Assembly-CSharp
- Assembly-CSharp-Editor
