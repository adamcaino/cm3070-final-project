using System.Collections.Generic;
using UnityEngine;

public class BossRoomEncounter : RoomEncounter
{
  [SerializeField] Health bossHealth;
  [SerializeField, Min(0f)] float victoryMusicFadeDuration = 2f;

  BossHealthBarUI healthUI;
  MusicPlayer musicPlayer;
  EnemyController bossController;
  BossSet bossSet;
  bool hasClearedEncounter;

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

  public void Configure(Health health, IReadOnlyList<Door> doors, BossSet configuredBossSet)
  {
    if (bossHealth != null)
    {
      bossHealth.OnDied -= HandleBossDied;
    }

    bossHealth = health;
    bossHealth.OnDied += HandleBossDied;
    bossSet = configuredBossSet;
    SetDoors(doors);
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
    if (bossSet != null)
    {
      musicPlayer?.Play(bossSet.bossMusic);
    }
    bossController?.Wake();
  }

  void HandleEncounterCleared()
  {
    if (hasClearedEncounter) return;

    hasClearedEncounter = true;
    healthUI?.StopTracking();

    if (musicPlayer == null)
    {
      musicPlayer = FindFirstObjectByType<MusicPlayer>();
    }

    if (bossSet != null)
    {
      musicPlayer?.PlayCrossfade(bossSet.clearAmbienceMusic, victoryMusicFadeDuration);
    }
    GameOverSignal.RaiseVictory(gameObject.scene.name);
  }
}
