using UnityEngine;

// Stores wall prop variants and the seeded chance used by prop placement.
[CreateAssetMenu(fileName = "PropSet", menuName = "Dungeon/Prop Set")]
public class PropSet : ScriptableObject
{
  [Header("Wall Props")]
  public GameObject[] wallProps;
  [Range(0f, 1f)] public float wallPropChance = 0.15f;
}
