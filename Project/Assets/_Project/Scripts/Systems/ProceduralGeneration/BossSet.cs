using UnityEngine;

[CreateAssetMenu(fileName = "BossSet", menuName = "Dungeon/Boss Set")]
// Stores boss prefab variants, placeholder appearance, and encounter music.
public class BossSet : ScriptableObject
{
  [Header("Boss")]
  // Boss prefab variants selected by the boss placement pass.
  public GameObject[] bossPrefabs;
  // Placeholder colour used when no boss prefab is assigned.
  public Color placeholderColor = new Color(0.5f, 0f, 0.5f, 1f);

  [Header("Music")]
  // Music played when the player enters and starts the boss encounter.
  [Tooltip("Played once the player enters the boss room and its doors lock.")]
  public AudioClip bossMusic;
  // Music played after the boss is defeated and the encounter is cleared.
  [Tooltip("Played once the boss is defeated and its doors unlock.")]
  public AudioClip clearAmbienceMusic;
}
