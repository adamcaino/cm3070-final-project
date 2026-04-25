# Cellular Automata Generation Iteration Log

## Scope

This document records the implementation and baseline evolution of CellularAutomataDungeonGenerator for the 2D proof-of-concept comparison pipeline.

---

## Iteration 1: Initial CA implementation

### Goals

- Implement an organic cavern-style generator to compare against BSP.
- Keep generation deterministic with seed support.
- Render in the same 2D tile semantics used by BSP for fair comparison.

### Work completed

- Added random fill initialization with configurable wall chance.
- Added configurable smoothing passes using neighborhood wall counts.
- Added flood-fill region detection and retained only the largest floor region.
- Added representative door markers within the retained region.

### Expected output at this stage

- Cave-like floor regions with irregular topology.
- Grey floors, black walls, red markers for representative transition points.
- Reproducible output when using fixed seed and same settings.

---

## Iteration 2: Stabilization as comparison baseline

### Observation

The CA generator did not experience the BSP-specific corridor-door placement issues because it does not create explicit room-to-corridor graph links.

### Behavior retained

- Largest-region filtering to reduce disconnected islands.
- Door markers remain representative and not tied to BSP-style boundary crossings.

### Outcome

- CA remained stable throughout BSP debugging iterations.
- Served as a consistent baseline for comparing structured vs organic generation.

---

## Final CA behavior

- Produces organic cavern-like layouts.
- Maintains deterministic behavior with seed control.
- Keeps one main navigable region by pruning smaller disconnected regions.
- Uses representative red markers rather than strict corridor-connected door semantics.

---

## Validation

Build verification after project iterations succeeded for:

- Assembly-CSharp
- Assembly-CSharp-Editor
