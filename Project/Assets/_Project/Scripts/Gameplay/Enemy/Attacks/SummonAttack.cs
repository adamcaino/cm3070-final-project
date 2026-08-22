using UnityEngine;
using UnityEngine.AI;

public class SummonAttack : AttackBase
{
  [SerializeField] GameObject[] minionPrefabs;
  [SerializeField, Min(1)] int minionCount = 1;
  [SerializeField, Min(0f)] float spawnRadius = 2f;

  protected override void OnExecute(EnemyController enemy)
  {
    PlayAttackAnimation(enemy);

    if (minionPrefabs != null && minionPrefabs.Length > 0)
    {
      for (int i = 0; i < minionCount; i++)
      {
        GameObject prefab = minionPrefabs[Random.Range(0, minionPrefabs.Length)];
        Vector3 candidate = enemy.transform.position + (Random.insideUnitSphere * spawnRadius);
        Vector3 spawnPosition = NavMesh.SamplePosition(candidate, out NavMeshHit hit, spawnRadius, NavMesh.AllAreas)
          ? hit.position
          : enemy.transform.position;

        Instantiate(prefab, spawnPosition, Quaternion.identity);
      }
    }

    RaiseAttackComplete();
  }
}
