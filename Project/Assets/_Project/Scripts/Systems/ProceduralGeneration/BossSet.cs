using UnityEngine;

[CreateAssetMenu(fileName = "BossSet", menuName = "Dungeon/Boss Set")]
public class BossSet : ScriptableObject
{
  [Header("Boss")]
  public GameObject[] bossPrefabs;
  public Color placeholderColor = new Color(0.5f, 0f, 0.5f, 1f);

  [Header("Music")]
  [Tooltip("Played once the player enters the boss room and its doors lock.")]
  public AudioClip bossMusic;
  [Tooltip("Played once the boss is defeated and its doors unlock.")]
  public AudioClip clearAmbienceMusic;
}
