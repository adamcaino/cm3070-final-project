using System.Collections.Generic;
using UnityEngine;

public class BossRoomEncounter : RoomEncounter
{
  [SerializeField] Health bossHealth;
  [SerializeField] AudioClip bossMusic;
  [SerializeField] AudioClip clearAmbienceMusic;

  BossHealthBarUI healthUI;
  MusicPlayer musicPlayer;
  EnemyController bossController;

  void Awake()
  {
    bossController = GetComponent<EnemyController>();
  }

  void OnEnable()
  {
    OnEncounterStarted += HandleEncounterStarted;
    OnEncounterCleared += HandleEncounterCleared;

    if (bossHealth != null)
    {
      bossHealth.OnDied += HandleBossDied;
    }
  }

  void OnDisable()
  {
    OnEncounterStarted -= HandleEncounterStarted;
    OnEncounterCleared -= HandleEncounterCleared;

    if (bossHealth != null)
    {
      bossHealth.OnDied -= HandleBossDied;
    }
  }

  public void Configure(Health health, IReadOnlyList<Door> doors)
  {
    if (bossHealth != null)
    {
      bossHealth.OnDied -= HandleBossDied;
    }

    bossHealth = health;
    bossHealth.OnDied += HandleBossDied;
    SetDoors(doors);
  }

  public void SetMusic(AudioClip encounterMusic, AudioClip ambienceMusic)
  {
    bossMusic = encounterMusic;
    clearAmbienceMusic = ambienceMusic;
  }

  void HandleBossDied() => CompleteEncounter();

  void HandleEncounterStarted()
  {

    if (healthUI == null)
    {
      healthUI = FindFirstObjectByType<BossHealthBarUI>(FindObjectsInactive.Include);
    }

    if (musicPlayer == null)
    {
      musicPlayer = FindFirstObjectByType<MusicPlayer>();
    }

    healthUI?.BeginTracking(bossHealth);
    musicPlayer?.Play(bossMusic);
    bossController?.Wake();
  }

  void HandleEncounterCleared()
  {
    healthUI?.StopTracking();
    musicPlayer?.Play(clearAmbienceMusic);
  }
}
