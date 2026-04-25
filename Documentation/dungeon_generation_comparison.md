# Proof of Concept Comparison: BSP vs Cellular Automata for Dungeon Generation

## Purpose of the comparison

Two proof-of-concept dungeon generators were implemented in Unity using a **2D grid visualisation** before committing to a later 3D prefab-based workflow. This was done intentionally because a 2D representation makes it easier to validate algorithm correctness, inspect connectivity, and compare layout qualities without the extra complexity of mesh placement, collision, or art production.

For the prototype visualisation:

- **Grey tiles** represent floor.
- **Black tiles** represent walls.
- **Red tiles** represent doors or representative transition points.

This allows both algorithms to be assessed on the same visual basis.

---

## Algorithms compared

### 1. Binary Space Partitioning (BSP)
BSP recursively divides the map area into smaller rectangular partitions. A room is then placed inside each final partition, and neighbouring partitions are connected with corridors.

### 2. Cellular Automata (CA)
Cellular automata begin with a random distribution of wall and floor cells. Repeated smoothing passes then transform this noisy map into cave-like open spaces.

---

## Why both were prototyped in 2D first

Using 2D first was the most appropriate choice for this stage because:

- it reduces implementation overhead;
- it makes debugging and inspection easier;
- it exposes the underlying topology clearly;
- it supports fair comparison before investing in 3D prefab pipelines;
- it allows the procedural logic to be evaluated independently of art assets.

This is especially useful in an academic setting because the algorithm can be analysed directly rather than being obscured by presentation details.

---

## Comparative analysis

| Criterion | BSP | Cellular Automata |
|---|---|---|
| Layout style | Structured, room-and-corridor | Organic, cave-like |
| Control over space | High | Moderate |
| Ease of guaranteeing connectivity | High | Moderate; often requires cleanup |
| Suitability for modular 3D prefabs | Very high | Lower |
| Deterministic reproduction with seed | Strong | Strong, but more sensitive to parameter changes |
| Natural door placement | Straightforward | Less natural |
| Visual variety | Moderate | High |
| Ease of academic explanation | High | High, but behaviour can be less intuitive |
| Suitability for tactical or room-based gameplay | High | Moderate |
| Suitability for caves or natural ruins | Moderate | High |

---

## Strengths and weaknesses of BSP

### Strengths
- Strong structural control over room placement.
- Easy to guarantee traversable layouts.
- Naturally supports corridors, rooms, and doors.
- Well suited to deterministic generation for multiplayer or save/load systems.
- Highly compatible with future 3D room-prefab workflows.

### Weaknesses
- Can look regular or grid-like.
- Produces less natural environments.
- May need extra variation passes if a more organic feel is desired.

---

## Strengths and weaknesses of Cellular Automata

### Strengths
- Produces more organic and natural cave shapes.
- Good visual variety from a relatively small ruleset.
- Suitable for subterranean, ruin, or wilderness-style maps.

### Weaknesses
- Less direct control over room structure.
- Doors and corridor semantics are not intrinsic to the method.
- Additional processing is often needed to remove isolated regions or ensure good navigation.
- Harder to translate cleanly into a modular room-based 3D prefab system.

---

## Interpretation for this project

If the long-term goal is to promote the chosen method into a **3D prefab-based dungeon system**, then BSP is the stronger candidate overall.

The main reason is that BSP generates spaces that already resemble the way modular 3D levels are typically constructed: discrete rooms linked by readable corridors. This makes it easier to:

- attach gameplay logic to rooms;
- place interactable doors between zones;
- spawn props, enemies, and objectives in controlled locations;
- support deterministic seeds in multiplayer sessions;
- later replace 2D tiles with authored 3D prefabs.

Cellular automata remain valuable, especially if the design goal shifts toward caves or more natural-looking underground environments. However, for a dungeon project that will likely use explicit rooms, doors, and network-consistent generation, CA is better viewed as a secondary option or as a complementary algorithm for specific biome types.

---

## Recommended direction going forward

### Recommended primary algorithm: BSP

BSP is the recommended approach for further development because it offers the best combination of:

- deterministic reproducibility,
- layout control,
- implementation clarity,
- network suitability,
- and compatibility with future 3D modular content.

### Recommended future use of CA

Cellular automata should still be retained as a useful experimental or secondary generator, particularly for:

- cave sublevels,
- underground transitions,
- hidden areas,
- or environments where irregularity is more important than room clarity.

---

## Conclusion

Both BSP and cellular automata are valid procedural generation techniques, but they serve different design purposes. For this project, the evidence from the 2D proof-of-concept comparison suggests that **BSP is the more appropriate foundation** for continued development into a 3D networked dungeon workflow. CA remains a strong option for organic environments, but BSP better satisfies the current requirements of structure, readability, determinism, and extensibility.
