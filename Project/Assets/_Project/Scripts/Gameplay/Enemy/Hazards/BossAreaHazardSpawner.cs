using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BossRoomEncounter))]
// Spawns timed area hazards at the player's position during a boss encounter.
public class BossAreaHazardSpawner : MonoBehaviour
{
  [SerializeField] GameObject hazardPrefab;
  [SerializeField, Min(0f)] float phaseOneInterval = 4f;
  [SerializeField, Min(0f)] float phaseTwoInterval = 2f;
  [SerializeField, Min(0)] int hazardDamage = 1;

  BossRoomEncounter roomEncounter;
  BossPhaseController phaseController;
  EnemyController enemyController;

  float currentInterval;
  Coroutine spawnRoutine;
  readonly List<BossAreaHazardController> activeHazards = new List<BossAreaHazardController>();

  // Caches encounter, phase, and enemy components and initializes the phase-one interval.
  void Awake()
  {
    roomEncounter = GetComponent<BossRoomEncounter>();
    phaseController = GetComponent<BossPhaseController>();
    enemyController = GetComponent<EnemyController>();

    currentInterval = phaseOneInterval;
  }

  // Subscribes to encounter and boss phase events.
  void OnEnable()
  {
    roomEncounter.OnEncounterStarted += HandleEncounterStarted;
    roomEncounter.OnEncounterCleared += HandleEncounterCleared;

    if (phaseController != null)
    {
      phaseController.OnStageChanged += HandleStageChanged;
    }
  }

  // Removes event subscriptions and stops all active hazard spawning.
  void OnDisable()
  {
    roomEncounter.OnEncounterStarted -= HandleEncounterStarted;
    roomEncounter.OnEncounterCleared -= HandleEncounterCleared;

    if (phaseController != null)
    {
      phaseController.OnStageChanged -= HandleStageChanged;
    }

    StopSpawning();
  }

  // Starts the hazard spawn routine when the encounter begins.
  void HandleEncounterStarted()
  {
    if (spawnRoutine == null)
    {
      spawnRoutine = StartCoroutine(SpawnRoutine());
    }
  }

  // Stops spawning and removes active hazards when the encounter is cleared.
  void HandleEncounterCleared() => StopSpawning();

  // Selects the spawn interval associated with the current boss stage.
  void HandleStageChanged(int stage)
  {
    currentInterval = stage >= 2 ? phaseTwoInterval : phaseOneInterval;
  }

  // Stops the spawn coroutine and removes every tracked active hazard.
  void StopSpawning()
  {
    if (spawnRoutine != null)
    {
      StopCoroutine(spawnRoutine);
      spawnRoutine = null;
    }

    StopActiveHazards();
  }

  // Waits between spawns while the encounter remains active.
  IEnumerator SpawnRoutine()
  {
    while (true)
    {
      yield return new WaitForSeconds(currentInterval);
      SpawnHazard();
    }
  }

  // Creates a hazard at the player's current position and configures its damage.
  void SpawnHazard()
  {
    if (hazardPrefab == null || enemyController == null || enemyController.Player == null) return;

    Vector3 spawnPosition = enemyController.Player.position + Vector3.up * 0.1f;
    GameObject hazard = Instantiate(hazardPrefab, spawnPosition, Quaternion.identity);

    BossAreaHazardController hazardController = hazard.GetComponent<BossAreaHazardController>();
    if (hazardController != null)
    {
      activeHazards.Add(hazardController);
    }

    // IAreaHazard implementations live on a child of the hazard root (e.g. BeeSwarmHazard sits next to
    // its own trigger collider), so this has to search the hierarchy rather than the root GameObject alone.
    IAreaHazard areaHazard = hazard.GetComponentInChildren<IAreaHazard>();
    if (areaHazard != null)
    {
      areaHazard.Configure(hazardDamage);
    }
  }

  // Stops and removes hazards tracked by this spawner.
  void StopActiveHazards()
  {
    for (int i = activeHazards.Count - 1; i >= 0; i--)
    {
      BossAreaHazardController hazard = activeHazards[i];
      if (hazard == null)
      {
        activeHazards.RemoveAt(i);
        continue;
      }

      hazard.StopHazard();
      activeHazards.RemoveAt(i);
    }
  }
}
