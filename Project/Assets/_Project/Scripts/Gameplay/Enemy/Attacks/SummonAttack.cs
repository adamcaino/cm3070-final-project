using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SummonAttack : AttackBase
{
  [Header("Summon Settings")]
  [SerializeField] AudioClip summonChargeSfx;
  [SerializeField] ParticleSystem summonChargeVfx;
  [SerializeField] GameObject minionSpawnPrefab;
  [SerializeField, Min(1)] int minionCount = 3;
  [SerializeField, Min(0f)] float spawnRadius = 2f;

  [Header("Debug")]
  [SerializeField] List<GameObject> activeMinions = new();


  public override bool CanExecute => PruneDeadMinions() <= 1;

  protected override void OnExecute(EnemyController enemy)
  {
    PlayAttackAnimation(enemy);

    if (summonChargeSfx != null)
    {
      GetComponent<AudioSource>()?.PlayOneShot(summonChargeSfx);
    }
  }



  public void SummonMinions()
  {
    int spawnCount = minionCount - PruneDeadMinions();

    if (minionSpawnPrefab != null)
    {
      for (int i = 0; i < spawnCount; i++)
      {
        Vector3 candidate = transform.position + (Random.insideUnitSphere * spawnRadius);
        Vector3 spawnPosition = NavMesh.SamplePosition(candidate, out NavMeshHit hit, spawnRadius, NavMesh.AllAreas)
          ? hit.position
          : transform.position;

        GameObject spawned = Instantiate(minionSpawnPrefab, spawnPosition, Quaternion.identity);

        if (spawned.TryGetComponent(out SpawnEnemy spawner))
        {
          spawner.OnEnemySpawned += enemy => activeMinions.Add(enemy);
        }
        else
        {
          activeMinions.Add(spawned);
        }
      }
    }

    RaiseAttackComplete();
  }

  public void PlaySummonAnimation(bool isPlaying)
  {
    if (isPlaying)
    {
      summonChargeVfx?.Play();
    }
    else
    {
      summonChargeVfx?.Stop();
    }
  }

  int PruneDeadMinions()
  {
    activeMinions.RemoveAll(minion => minion == null);
    return activeMinions.Count;
  }
}
