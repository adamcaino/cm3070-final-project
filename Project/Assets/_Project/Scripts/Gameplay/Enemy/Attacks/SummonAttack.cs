using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Summons a configured number of minions while tracking active summoned enemies.
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

  // Starts the summon animation and plays its charge sound.
  protected override void OnExecute(EnemyController enemy)
  {
    PlayAttackAnimation(enemy);

    if (summonChargeSfx != null)
    {
      GetComponent<AudioSource>()?.PlayOneShot(summonChargeSfx);
    }
  }



  // Spawns available minions at sampled NavMesh positions and completes the attack.
  public void SummonMinions()
  {
    int spawnCount = minionCount - PruneDeadMinions();

    if (minionSpawnPrefab != null)
    {
      for (int i = 0; i < spawnCount; i++)
      {
        Vector3 candidate = transform.position + (Random.insideUnitSphere * spawnRadius);
        if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, spawnRadius, NavMesh.AllAreas))
        {
          continue;
        }

        GameObject spawned = Instantiate(minionSpawnPrefab, hit.position, Quaternion.identity);

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

  // Starts or stops the summon charge visual effect.
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

  // Removes destroyed minions from the active list and returns the remaining count.
  int PruneDeadMinions()
  {
    activeMinions.RemoveAll(minion => minion == null);
    return activeMinions.Count;
  }
}
