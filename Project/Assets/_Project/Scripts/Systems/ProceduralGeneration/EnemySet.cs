using UnityEngine;

/// <summary>
/// Enemy library the enemy placement pass reads from. Every RoomRole.Normal room picks a single prefab
/// from this array for the whole room (one enemy type per room, no mixing) and spawns however many copies
/// its floor area calls for. Leave the array empty to fall back to a placeholder primitive of the matching
/// color, same convention as DungeonTileSet/PropSet/PointOfInterestSet.
/// </summary>
[CreateAssetMenu(fileName = "EnemySet", menuName = "Dungeon/Enemy Set")]
public class EnemySet : ScriptableObject
{
  [Header("Enemies")]
  public GameObject[] enemyPrefabs;
  public GameObject[] toughEnemyPrefabs;
  public Color placeholderColor = new Color(0.8f, 0.1f, 0.1f, 1f);

  [Header("Density")]
  [Tooltip("Room floor area (in tiles) per enemy - lower spawns more enemies for the same room size.")]
  [Min(1f)] public float tilesPerEnemy = 6f;
  [Tooltip("Minimum enemies for rooms closest to spawn.")]
  [Min(1)] public int minEnemiesNearSpawn = 2;
  [Tooltip("Maximum enemies for rooms closest to spawn.")]
  [Min(1)] public int maxEnemiesNearSpawn = 3;
  [Tooltip("Minimum enemies for rooms closest to the boss.")]
  [Min(1)] public int minEnemiesNearBoss = 3;
  [Tooltip("Maximum enemies for rooms closest to the boss.")]
  [Min(1)] public int maxEnemiesNearBoss = 6;

  [Header("Tough Enemies")]
  [Tooltip("Normalized distance from spawn-to-boss path where tough enemies start being eligible.")]
  [Range(0f, 1f)] public float toughEnemyStartDistance = 0.5f;
  [Tooltip("Absolute cap of tough enemies in a room.")]
  [Range(0, 2)] public int maxToughEnemiesPerRoom = 2;
}
