using System.Collections.Generic;
using UnityEngine;

// Starts the boss encounter, tracks its health, controls encounter music, and reports victory.
public class BossRoomEncounter : RoomEncounter
{
  [SerializeField] Health bossHealth;
  [SerializeField, Min(0f)] float victoryMusicFadeDuration = 2f;

  BossHealthBarUI healthUI;
  MusicPlayer musicPlayer;
  EnemyController bossController;
  BossSet bossSet;
  bool hasClearedEncounter;

  // Caches the enemy controller attached to the boss room.
  void Awake()
  {
    bossController = GetComponent<EnemyController>();
  }

  // Subscribes to encounter and boss-health events when the room becomes active.
  void OnEnable()
  {
    OnEncounterStarted += HandleEncounterStarted;
    OnEncounterCleared += HandleEncounterCleared;

    if (bossHealth != null)
    {
      bossHealth.OnDied += HandleBossDied;
    }
  }

  // Removes encounter and boss-health event subscriptions when the room is disabled.
  void OnDisable()
  {
    OnEncounterStarted -= HandleEncounterStarted;
    OnEncounterCleared -= HandleEncounterCleared;

    if (bossHealth != null)
    {
      bossHealth.OnDied -= HandleBossDied;
    }
  }

  // Assigns the runtime boss, encounter doors, and music configuration.
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

  // Completes the encounter when the configured boss dies.
  void HandleBossDied() => CompleteEncounter();

  // Begins boss tracking, plays boss music, and wakes the boss controller.
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

  // Stops boss tracking, plays the victory ambience, and raises the victory signal once.
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
