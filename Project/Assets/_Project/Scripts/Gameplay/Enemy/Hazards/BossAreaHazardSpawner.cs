using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BossRoomEncounter))]
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

  void Awake()
  {
    roomEncounter = GetComponent<BossRoomEncounter>();
    phaseController = GetComponent<BossPhaseController>();
    enemyController = GetComponent<EnemyController>();

    currentInterval = phaseOneInterval;
  }

  void OnEnable()
  {
    roomEncounter.OnEncounterStarted += HandleEncounterStarted;
    roomEncounter.OnEncounterCleared += HandleEncounterCleared;

    if (phaseController != null)
    {
      phaseController.OnStageChanged += HandleStageChanged;
    }
  }

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

  void HandleEncounterStarted()
  {
    if (spawnRoutine == null)
    {
      spawnRoutine = StartCoroutine(SpawnRoutine());
    }
  }

  void HandleEncounterCleared() => StopSpawning();

  void HandleStageChanged(int stage)
  {
    currentInterval = stage >= 2 ? phaseTwoInterval : phaseOneInterval;
  }

  void StopSpawning()
  {
    if (spawnRoutine != null)
    {
      StopCoroutine(spawnRoutine);
      spawnRoutine = null;
    }
  }

  IEnumerator SpawnRoutine()
  {
    while (true)
    {
      yield return new WaitForSeconds(currentInterval);
      SpawnHazard();
    }
  }

  void SpawnHazard()
  {
    if (hazardPrefab == null || enemyController == null || enemyController.Player == null) return;

    Vector3 spawnPosition = enemyController.Player.position + Vector3.up * 0.1f;
    GameObject hazard = Instantiate(hazardPrefab, spawnPosition, Quaternion.identity);

    // IAreaHazard implementations live on a child of the hazard root (e.g. BeeSwarmHazard sits next to
    // its own trigger collider), so this has to search the hierarchy rather than the root GameObject alone.
    IAreaHazard areaHazard = hazard.GetComponentInChildren<IAreaHazard>();
    if (areaHazard != null)
    {
      areaHazard.Configure(hazardDamage);
    }
  }
}
